﻿using System;
using System.Collections.Generic;
using System.Numerics;
using SkiaSharp;

namespace Waher.Script.Graphs3D
{
	/// <summary>
	/// 3D drawing area.
	/// </summary>
	public class Canvas3D
	{
		private readonly byte[] pixels;
		private readonly float[] zBuffer;
		private readonly float[] xBuf;
		private readonly float[] yBuf;
		private readonly float[] zBuf;
		private readonly Vector3[] normalBuf;
		private readonly SKColor[] colorBuf;
		private Vector4 viewerPosition;
		private Matrix4x4 projectionTransformation;
		private Matrix4x4 modelTransformation;
		private Vector4 last = Vector4.Zero;
		private readonly int width;
		private readonly int height;
		private readonly int overSampling;
		private readonly int w;
		private readonly int h;
		private readonly int wm1;
		private readonly int hm1;
		private readonly int cx;
		private readonly int cy;
		private float distance;

		/// <summary>
		/// 3D drawing area.
		/// 
		/// By default, the camera is looking along the z-axis, with no projection, and no scaling.
		/// The center of the canvas is located at origo.
		/// </summary>
		/// <param name="Width">Width of area, in pixels.</param>
		/// <param name="Height">Height of area, in pixels.</param>
		/// <param name="OverSampling">Number of subpixels for each generated pixel.
		/// Oversampling provides a means to achieve anti-aliasing in the rendered result.</param>
		/// <param name="BackgroundColor">Background color</param>
		public Canvas3D(int Width, int Height, int OverSampling, SKColor BackgroundColor)
		{
			if (Width <= 0)
				throw new ArgumentOutOfRangeException("Width must be a positive integer.", nameof(Width));

			if (Height <= 0)
				throw new ArgumentOutOfRangeException("Height must be a positive integer.", nameof(Height));

			if (OverSampling <= 0)
				throw new ArgumentOutOfRangeException("Oversampling must be a positive integer.", nameof(OverSampling));

			this.width = Width;
			this.height = Height;
			this.overSampling = OverSampling;
			this.w = Width * OverSampling;
			this.h = Height * OverSampling;
			this.wm1 = this.w - 1;
			this.hm1 = this.h - 1;
			this.cx = this.w / 2;
			this.cy = this.h / 2;
			this.ResetTransforms();

			int i, j, c = this.w * this.h;
			byte R = BackgroundColor.Red;
			byte G = BackgroundColor.Green;
			byte B = BackgroundColor.Blue;
			byte A = BackgroundColor.Alpha;

			this.pixels = new byte[c * 4];
			this.zBuffer = new float[c];
			this.xBuf = new float[this.w];
			this.yBuf = new float[this.w];
			this.zBuf = new float[this.w];
			this.normalBuf = new Vector3[this.w];
			this.colorBuf = new SKColor[this.w];

			for (i = j = 0; i < c; i++)
			{
				this.pixels[j++] = R;
				this.pixels[j++] = G;
				this.pixels[j++] = B;
				this.pixels[j++] = A;

				this.zBuffer[i] = float.MaxValue;
			}
		}

		#region Colors

		private static uint ToUInt(SKColor Color)
		{
			uint Result = Color.Alpha;
			Result <<= 8;
			Result |= Color.Blue;
			Result <<= 8;
			Result |= Color.Green;
			Result <<= 8;
			Result |= Color.Red;

			return Result;
		}

		private static SKColor ToColor(uint Color)
		{
			byte R = (byte)Color;
			Color >>= 8;
			byte G = (byte)Color;
			Color >>= 8;
			byte B = (byte)Color;
			Color >>= 8;
			byte A = (byte)Color;

			return new SKColor(R, G, B, A);
		}

		#endregion

		#region Bitmaps

		/// <summary>
		/// Creates a bitmap from the pixels in the canvas.
		/// </summary>
		/// <returns></returns>
		public SKImage GetBitmap()
		{
			if (this.overSampling == 1)
				return this.GetBitmap(this.pixels);
			else
			{
				byte[] Pixels = new byte[this.width * this.height * 4];
				int x, y, dx, dy, p0, p, q = 0;
				int o2 = this.overSampling * this.overSampling;
				int h = o2 >> 1;
				uint SumR, SumG, SumB, SumA;

				for (y = 0; y < this.height; y++)
				{
					for (x = 0; x < this.width; x++)
					{
						SumR = SumG = SumB = SumA = 0;
						p0 = ((y * this.w) + x) * this.overSampling * 4;

						for (dy = 0; dy < this.overSampling; dy++, p0 += this.w * 4)
						{
							for (dx = 0, p = p0; dx < this.overSampling; dx++)
							{
								SumR += this.pixels[p++];
								SumG += this.pixels[p++];
								SumB += this.pixels[p++];
								SumA += this.pixels[p++];
							}
						}

						Pixels[q++] = (byte)((SumR + h) / o2);
						Pixels[q++] = (byte)((SumG + h) / o2);
						Pixels[q++] = (byte)((SumB + h) / o2);
						Pixels[q++] = (byte)((SumA + h) / o2);
					}
				}

				return this.GetBitmap(Pixels);
			}
		}

		private SKImage GetBitmap(byte[] Pixels)
		{
			using (SKData Data = SKData.CreateCopy(Pixels))
			{
				SKImageInfo ImageInfo = new SKImageInfo(this.width, this.height, SKColorType.Rgba8888, SKAlphaType.Premul);
				return SKImage.FromPixels(ImageInfo, Data, this.width << 2);
			}
		}

		#endregion

		#region Projection Transformations

		/// <summary>
		/// Resets any transforms.
		/// </summary>
		public void ResetTransforms()
		{
			this.distance = 0;
			this.viewerPosition = new Vector4(0, 0, 0, 1);
			this.projectionTransformation = Matrix4x4.CreateTranslation(this.cx, this.cy, 0);
			this.projectionTransformation = Matrix4x4.CreateScale(-this.overSampling, this.overSampling, 1) * this.projectionTransformation;
			this.modelTransformation = Matrix4x4.Identity;
		}

		/// <summary>
		/// Current projection transformation matrix.
		/// </summary>
		public Matrix4x4 ProjectionTransformation
		{
			get => this.projectionTransformation;
			set => this.projectionTransformation = value;
		}

		/// <summary>
		/// Applies a perspective projection.
		/// </summary>
		/// <param name="NearPlaneDistance">Distance between near projection plane and camera.</param>
		/// <param name="FarPlaneDistance">Distance between far projection plane and camera.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 Perspective(float NearPlaneDistance, float FarPlaneDistance)
		{
			if (NearPlaneDistance <= 0)
				throw new ArgumentOutOfRangeException("Invalid camera distance.", nameof(NearPlaneDistance));

			if (FarPlaneDistance <= NearPlaneDistance)
				throw new ArgumentOutOfRangeException("Invalid camera distance.", nameof(FarPlaneDistance));

			Matrix4x4 Prev = this.projectionTransformation;
			this.projectionTransformation = Matrix4x4.CreatePerspective(1, 1, NearPlaneDistance, FarPlaneDistance) * this.projectionTransformation;
			this.distance = NearPlaneDistance;
			this.viewerPosition = new Vector4(0, 0, -this.distance, 1);

			return Prev;
		}

		/// <summary>
		/// Viewer position
		/// </summary>
		public Vector4 ViewerPosition => this.viewerPosition;

		/// <summary>
		/// Transforms coordinates to screen coordinates.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>Transformed point.</returns>
		public Vector3 Project(Vector4 Point)
		{
			Vector4 v = Vector4.Transform(Point, this.projectionTransformation);
			float d = 1f / v.W;
			return new Vector3(v.X * d, v.Y * d, v.Z * d);
		}

		/// <summary>
		/// Transforms a world coordinate to a display coordinate.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>Transformed point.</returns>
		public Vector3 Project(Vector3 Point)
		{
			return Vector3.Transform(Point, this.projectionTransformation);
		}

		#endregion

		#region Model Transformations

		/// <summary>
		/// Current model transformation matrix.
		/// </summary>
		public Matrix4x4 ModelTransformation
		{
			get => this.modelTransformation;
			set => this.modelTransformation = value;
		}

		/// <summary>
		/// Transforms a world coordinate to a display coordinate.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>Transformed point.</returns>
		public Vector4 ModelTransform(Vector4 Point)
		{
			return Vector4.Transform(Point, this.modelTransformation);
		}

		/// <summary>
		/// Transforms a world coordinate to a display coordinate.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <returns>Transformed point.</returns>
		public Vector3 ModelTransform(Vector3 Point)
		{
			return Vector3.Transform(Point, this.modelTransformation);
		}

		/// <summary>
		/// Rotates the world around the X-axis.
		/// </summary>
		/// <param name="Degrees">Degrees</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 RotateX(float Degrees)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateRotationX(Degrees * degToRad) * this.modelTransformation;
			return Prev;
		}

		private const float degToRad = (float)(Math.PI / 180);

		/// <summary>
		/// Rotates the world around an axis parallel to the X-axis, going through
		/// the center point <paramref name="CenterPoint"/>.
		/// </summary>
		/// <param name="Degrees">Degrees</param>
		/// <param name="CenterPoint">Center point.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 RotateX(float Degrees, Vector3 CenterPoint)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateRotationX(Degrees * degToRad, CenterPoint) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Rotates the world around the Y-axis.
		/// </summary>
		/// <param name="Degrees">Degrees</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 RotateY(float Degrees)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateRotationY(Degrees * degToRad) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Rotates the world around an axis parallel to the Y-axis, going through
		/// the center point <paramref name="CenterPoint"/>.
		/// </summary>
		/// <param name="Degrees">Degrees</param>
		/// <param name="CenterPoint">Center point.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 RotateY(float Degrees, Vector3 CenterPoint)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateRotationY(Degrees * degToRad, CenterPoint) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Rotates the world around the Z-axis.
		/// </summary>
		/// <param name="Degrees">Degrees</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 RotateZ(float Degrees)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateRotationZ(Degrees * degToRad) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Rotates the world around an axis parallel to the Z-axis, going through
		/// the center point <paramref name="CenterPoint"/>.
		/// </summary>
		/// <param name="Degrees">Degrees</param>
		/// <param name="CenterPoint">Center point.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 RotateZ(float Degrees, Vector3 CenterPoint)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateRotationZ(Degrees * degToRad, CenterPoint) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Scales the world
		/// </summary>
		/// <param name="Scale">Scale</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 Scale(float Scale)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateScale(Scale) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Scales the world
		/// </summary>
		/// <param name="Scale">Scale</param>
		/// <param name="CenterPoint">Center point.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 Scale(float Scale, Vector3 CenterPoint)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateScale(Scale, CenterPoint) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Scales the world
		/// </summary>
		/// <param name="ScaleX">Scale along X-axis.</param>
		/// <param name="ScaleY">Scale along Y-axis.</param>
		/// <param name="ScaleZ">Scale along Z-axis.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 Scale(float ScaleX, float ScaleY, float ScaleZ)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateScale(ScaleX, ScaleY, ScaleZ) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Scales the world
		/// </summary>
		/// <param name="ScaleX">Scale along X-axis.</param>
		/// <param name="ScaleY">Scale along Y-axis.</param>
		/// <param name="ScaleZ">Scale along Z-axis.</param>
		/// <param name="CenterPoint">Center point.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 Scale(float ScaleX, float ScaleY, float ScaleZ, Vector3 CenterPoint)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateScale(ScaleX, ScaleY, ScaleZ, CenterPoint) * this.modelTransformation;
			return Prev;
		}

		/// <summary>
		/// Translates the world.
		/// </summary>
		/// <param name="DeltaX">Movement along the X-axis.</param>
		/// <param name="DelayY">Movement along the Y-axis.</param>
		/// <param name="DeltaZ">Movement along the Z-axis.</param>
		/// <returns>Previous model transformation matrix.</returns>
		public Matrix4x4 Translate(float DeltaX, float DelayY, float DeltaZ)
		{
			Matrix4x4 Prev = this.modelTransformation;
			this.modelTransformation = Matrix4x4.CreateTranslation(DeltaX, DelayY, DeltaZ) * this.modelTransformation;
			return Prev;
		}

		#endregion

		#region Plot

		/// <summary>
		/// Plots a point on the 3D-canvas.
		/// </summary>
		/// <param name="Point">Point to plot.</param>
		/// <param name="Color">Color.</param>
		public void Plot(Vector4 Point, SKColor Color)
		{
			this.Plot(Point, ToUInt(Color));
		}

		/// <summary>
		/// Plots a point on the 3D-canvas.
		/// </summary>
		/// <param name="Point">Point to plot.</param>
		/// <param name="Color">Color.</param>
		public void Plot(Vector4 Point, uint Color)
		{
			this.last = Point;

			Vector4 WorldPoint = this.ModelTransform(Point);
			Vector3 ScreenPoint = this.Project(WorldPoint);
			if (ScreenPoint.Z >= 0)
				this.Plot((int)(ScreenPoint.X + 0.5f), (int)(ScreenPoint.Y + 0.5f), WorldPoint.Z, Color);
		}

		private void Plot(int x, int y, float z, uint Color)
		{
			if (x >= 0 && x < this.w && y >= 0 && y < this.h)
			{
				int p = y * this.w + x;

				if (z >= 0 && z < this.zBuffer[p])
				{
					this.zBuffer[p] = z;

					p <<= 2;

					byte A = (byte)(Color >> 24);
					if (A == 255)
					{
						this.pixels[p++] = (byte)Color;
						Color >>= 8;
						this.pixels[p++] = (byte)Color;
						Color >>= 8;
						this.pixels[p++] = (byte)Color;
						Color >>= 8;
						this.pixels[p] = (byte)Color;
					}
					else
					{
						byte R = (byte)Color;
						byte G = (byte)(Color >> 8);
						byte B = (byte)(Color >> 16);
						byte R2 = this.pixels[p++];
						byte G2 = this.pixels[p++];
						byte B2 = this.pixels[p++];
						byte A2 = this.pixels[p];
						byte R3, G3, B3, A3;

						if (A2 == 255)
						{
							R3 = (byte)(((R * A + R2 * (255 - A)) + 128) / 255);
							G3 = (byte)(((G * A + G2 * (255 - A)) + 128) / 255);
							B3 = (byte)(((B * A + B2 * (255 - A)) + 128) / 255);
							A3 = 255;
						}
						else
						{
							R2 = (byte)((R2 * A2 + 128) / 255);
							G2 = (byte)((G2 * A2 + 128) / 255);
							B2 = (byte)((B2 * A2 + 128) / 255);

							R3 = (byte)(((R * A + R2 * (255 - A)) + 128) / 255);
							G3 = (byte)(((G * A + G2 * (255 - A)) + 128) / 255);
							B3 = (byte)(((B * A + B2 * (255 - A)) + 128) / 255);
							A3 = (byte)(255 - (((255 - A) * (255 - A2) + 128) / 255));
						}

						this.pixels[p--] = A3;
						this.pixels[p--] = B3;
						this.pixels[p--] = G3;
						this.pixels[p] = R3;
					}
				}
			}
		}

		#endregion

		#region Lines

		private bool ClipLine(ref float x0, ref float y0, ref float z0,
			ref float x1, ref float y1, ref float z1)
		{
			byte Mask0 = 0;
			byte Mask1 = 0;
			float Delta;

			if (x0 < 0)
				Mask0 |= 1;
			else if (x0 > this.wm1)
				Mask0 |= 2;

			if (y0 < 0)
				Mask0 |= 4;
			else if (y0 > this.hm1)
				Mask0 |= 8;

			if (x1 < 0)
				Mask1 |= 1;
			else if (x1 > this.wm1)
				Mask1 |= 2;

			if (y1 < 0)
				Mask1 |= 4;
			else if (y1 > this.hm1)
				Mask1 |= 8;

			if (Mask0 == 0 && Mask1 == 0)
				return true;

			if ((Mask0 & Mask1) != 0)
				return false;

			// Left edge:

			if ((Mask0 & 1) != 0)
			{
				Delta = x0 / (x1 - x0);    // Divisor is non-zero, or masks would have common bit.
				y0 -= (y1 - y0) * Delta;
				z0 -= (z1 - z0) * Delta;
				x0 = 0;

				Mask0 &= 254;
				if (y0 < 0)
					Mask0 |= 4;
				else if (y0 > this.hm1)
					Mask0 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 1) != 0)
			{
				Delta = x1 / (x0 - x1);    // Divisor is non-zero, or masks would have common bit.
				y1 -= (y0 - y1) * Delta;
				z1 -= (z0 - z1) * Delta;
				x1 = 0;

				Mask1 &= 254;
				if (y1 < 0)
					Mask1 |= 4;
				else if (y1 > this.hm1)
					Mask1 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Top edge:

			if ((Mask0 & 4) != 0)
			{
				Delta = y0 / (y1 - y0);    // Divisor is non-zero, or masks would have common bit.
				x0 -= (x1 - x0) * Delta;
				z0 -= (z1 - z0) * Delta;
				y0 = 0;

				Mask0 &= 251;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 4) != 0)
			{
				Delta = y1 / (y0 - y1);    // Divisor is non-zero, or masks would have common bit.
				x1 -= (x0 - x1) * Delta;
				z1 -= (z0 - z1) * Delta;
				y1 = 0;

				Mask1 &= 251;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Right edge:

			if ((Mask0 & 2) != 0)
			{
				Delta = (this.wm1 - x0) / (x1 - x0);    // Divisor is non-zero, or masks would have common bit.
				y0 += (y1 - y0) * Delta;
				z0 += (z1 - z0) * Delta;
				x0 = this.wm1;

				Mask0 &= 253;
				if (y0 < 0)
					Mask0 |= 4;
				else if (y0 > this.hm1)
					Mask0 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 2) != 0)
			{
				Delta = (this.wm1 - x1) / (x0 - x1);    // Divisor is non-zero, or masks would have common bit.
				y1 += (y0 - y1) * Delta;
				z1 += (z0 - z1) * Delta;
				x1 = this.wm1;

				Mask1 &= 253;
				if (y1 < 0)
					Mask1 |= 4;
				else if (y1 > this.hm1)
					Mask1 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Bottom edge:

			if ((Mask0 & 8) != 0)
			{
				Delta = (this.hm1 - y0) / (y1 - y0);    // Divisor is non-zero, or masks would have common bit.
				x0 += (x1 - x0) * Delta;
				z0 += (z1 - z0) * Delta;
				y0 = this.hm1;

				Mask0 &= 247;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 8) != 0)
			{
				Delta = (this.hm1 - y1) / (y0 - y1);    // Divisor is non-zero, or masks would have common bit.
				x1 += (x0 - x1) * Delta;
				z1 += (z0 - z1) * Delta;
				y1 = this.hm1;

				Mask1 &= 247;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			return ((Mask0 | Mask1) == 0);
		}


		private bool ClipLine(ref float x0, ref float y0, ref float z0,
			ref float rx0, ref float ry0, ref float rz0,
			ref float x1, ref float y1, ref float z1,
			ref float rx1, ref float ry1, ref float rz1)
		{
			byte Mask0 = 0;
			byte Mask1 = 0;
			float Delta;

			if (x0 < 0)
				Mask0 |= 1;
			else if (x0 > this.wm1)
				Mask0 |= 2;

			if (y0 < 0)
				Mask0 |= 4;
			else if (y0 > this.hm1)
				Mask0 |= 8;

			if (x1 < 0)
				Mask1 |= 1;
			else if (x1 > this.wm1)
				Mask1 |= 2;

			if (y1 < 0)
				Mask1 |= 4;
			else if (y1 > this.hm1)
				Mask1 |= 8;

			if (Mask0 == 0 && Mask1 == 0)
				return true;

			if ((Mask0 & Mask1) != 0)
				return false;

			// Left edge:

			if ((Mask0 & 1) != 0)
			{
				Delta = x0 / (x1 - x0);    // Divisor is non-zero, or masks would have common bit.
				y0 -= (y1 - y0) * Delta;
				z0 -= (z1 - z0) * Delta;
				rx0 -= (rx1 - rx0) * Delta;
				ry0 -= (ry1 - ry0) * Delta;
				rz0 -= (rz1 - rz0) * Delta;
				x0 = 0;

				Mask0 &= 254;
				if (y0 < 0)
					Mask0 |= 4;
				else if (y0 > this.hm1)
					Mask0 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 1) != 0)
			{
				Delta = x1 / (x0 - x1);    // Divisor is non-zero, or masks would have common bit.
				y1 -= (y0 - y1) * Delta;
				z1 -= (z0 - z1) * Delta;
				rx1 -= (rx0 - rx1) * Delta;
				ry1 -= (ry0 - ry1) * Delta;
				rz1 -= (rz0 - rz1) * Delta;
				x1 = 0;

				Mask1 &= 254;
				if (y1 < 0)
					Mask1 |= 4;
				else if (y1 > this.hm1)
					Mask1 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Top edge:

			if ((Mask0 & 4) != 0)
			{
				Delta = y0 / (y1 - y0);    // Divisor is non-zero, or masks would have common bit.
				x0 -= (x1 - x0) * Delta;
				z0 -= (z1 - z0) * Delta;
				rx0 -= (rx1 - rx0) * Delta;
				ry0 -= (ry1 - ry0) * Delta;
				rz0 -= (rz1 - rz0) * Delta;
				y0 = 0;

				Mask0 &= 251;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 4) != 0)
			{
				Delta = y1 / (y0 - y1);    // Divisor is non-zero, or masks would have common bit.
				x1 -= (x0 - x1) * Delta;
				z1 -= (z0 - z1) * Delta;
				rx1 -= (rx0 - rx1) * Delta;
				ry1 -= (ry0 - ry1) * Delta;
				rz1 -= (rz0 - rz1) * Delta;
				y1 = 0;

				Mask1 &= 251;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Right edge:

			if ((Mask0 & 2) != 0)
			{
				Delta = (this.wm1 - x0) / (x1 - x0);    // Divisor is non-zero, or masks would have common bit.
				y0 += (y1 - y0) * Delta;
				z0 += (z1 - z0) * Delta;
				rx0 += (rx1 - rx0) * Delta;
				ry0 += (ry1 - ry0) * Delta;
				rz0 += (rz1 - rz0) * Delta;
				x0 = this.wm1;

				Mask0 &= 253;
				if (y0 < 0)
					Mask0 |= 4;
				else if (y0 > this.hm1)
					Mask0 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 2) != 0)
			{
				Delta = (this.wm1 - x1) / (x0 - x1);    // Divisor is non-zero, or masks would have common bit.
				y1 += (y0 - y1) * Delta;
				z1 += (z0 - z1) * Delta;
				rx1 += (rx0 - rx1) * Delta;
				ry1 += (ry0 - ry1) * Delta;
				rz1 += (rz0 - rz1) * Delta;
				x1 = this.wm1;

				Mask1 &= 253;
				if (y1 < 0)
					Mask1 |= 4;
				else if (y1 > this.hm1)
					Mask1 |= 8;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Bottom edge:

			if ((Mask0 & 8) != 0)
			{
				Delta = (this.hm1 - y0) / (y1 - y0);    // Divisor is non-zero, or masks would have common bit.
				x0 += (x1 - x0) * Delta;
				z0 += (z1 - z0) * Delta;
				rx0 += (rx1 - rx0) * Delta;
				ry0 += (ry1 - ry0) * Delta;
				rz0 += (rz1 - rz0) * Delta;
				y0 = this.hm1;

				Mask0 &= 247;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 8) != 0)
			{
				Delta = (this.hm1 - y1) / (y0 - y1);    // Divisor is non-zero, or masks would have common bit.
				x1 += (x0 - x1) * Delta;
				z1 += (z0 - z1) * Delta;
				rx1 += (rx0 - rx1) * Delta;
				ry1 += (ry0 - ry1) * Delta;
				rz1 += (rz0 - rz1) * Delta;
				y1 = this.hm1;

				Mask1 &= 247;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			return ((Mask0 | Mask1) == 0);
		}

		/// <summary>
		/// Draws a line between P0 and P1.
		/// </summary>
		/// <param name="P0">Point 1.</param>
		/// <param name="P1">Point 2.</param>
		/// <param name="Color">Color</param>
		public void Line(Vector4 P0, Vector4 P1, SKColor Color)
		{
			this.Line(P0, P1, ToUInt(Color));
		}

		/// <summary>
		/// Draws a line between P0 and P1.
		/// </summary>
		/// <param name="P0">Point 1.</param>
		/// <param name="P1">Point 2.</param>
		/// <param name="Color">Color</param>
		public void Line(Vector4 P0, Vector4 P1, uint Color)
		{
			this.last = P1;

			Vector4 WP0 = this.ModelTransform(P0);
			Vector4 WP1 = this.ModelTransform(P1);
			Vector3 SP0 = this.Project(WP0);
			Vector3 SP1 = this.Project(WP1);

			// TODO: Clip z=0

			float x0, y0, z0;
			float x1, y1, z1;

			x0 = SP0.X;
			y0 = SP0.Y;
			z0 = SP0.Z;

			x1 = SP1.X;
			y1 = SP1.Y;
			z1 = SP1.Z;

			if (this.ClipLine(ref x0, ref y0, ref z0, ref x1, ref y1, ref z1))
			{
				float dx = x1 - x0;
				float dy = y1 - y0;
				float dz;
				float temp;

				this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);

				if (Math.Abs(dy) >= Math.Abs(dx))
				{
					if (dy == 0)
						return;

					if (dy < 0)
					{
						temp = x0;
						x0 = x1;
						x1 = temp;

						temp = y0;
						y0 = y1;
						y1 = temp;

						temp = z0;
						z0 = z1;
						z1 = temp;

						dx = -dx;
						dy = -dy;
					}

					dz = (z1 - z0) / dy;
					dx /= dy;

					temp = 1 - (y0 - ((int)y0));
					y0 += temp;
					x0 += dx * temp;
					z0 += dz * temp;

					while (y0 <= y1)
					{
						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
						y0++;
						x0 += dx;
						z0 += dz;
					}

					temp = y1 - ((int)y1);
					if (temp > 0)
					{
						temp = 1 - temp;

						y0 -= temp;
						x0 -= dx * temp;
						z0 -= dz * temp;

						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
					}
				}
				else
				{
					if (dx < 0)
					{
						temp = x0;
						x0 = x1;
						x1 = temp;

						temp = y0;
						y0 = y1;
						y1 = temp;

						temp = z0;
						z0 = z1;
						z1 = temp;

						dx = -dx;
						dy = -dy;
					}

					dz = (z1 - z0) / dx;
					dy /= dx;

					temp = 1 - (x0 - ((int)x0));
					x0 += temp;
					y0 += dy * temp;
					z0 += dz * temp;

					while (x0 <= x1)
					{
						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
						x0++;
						y0 += dy;
						z0 += dz;
					}

					temp = x1 - ((int)x1);
					if (temp > 0)
					{
						temp = 1 - temp;

						x0 -= temp;
						y0 -= dy * temp;
						z0 -= dz * temp;

						this.Plot((int)(x0 + 0.5f), (int)(y0 + 0.5f), z0, Color);
					}
				}
			}
		}

		/// <summary>
		/// Moves to a point.
		/// </summary>
		/// <param name="Point">Point.</param>
		public void MoveTo(Vector4 Point)
		{
			this.last = Point;
		}

		/// <summary>
		/// Draws a line to <paramref name="Point"/> from the last endpoint.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <param name="Color">Color</param>
		public void LineTo(Vector4 Point, SKColor Color)
		{
			this.LineTo(Point, ToUInt(Color));
		}

		/// <summary>
		/// Draws a line to <paramref name="Point"/> from the last endpoint.
		/// </summary>
		/// <param name="Point">Point.</param>
		/// <param name="Color">Color</param>
		public void LineTo(Vector4 Point, uint Color)
		{
			this.Line(this.last, Point, Color);
		}

		/// <summary>
		/// Draws lines between a set of nodes.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Color">Color</param>
		public void PolyLine(Vector4[] Nodes, SKColor Color)
		{
			this.PolyLine(Nodes, ToUInt(Color));
		}

		/// <summary>
		/// Draws lines between a set of nodes.
		/// </summary>
		/// <param name="Nodes">Nodes</param>
		/// <param name="Color">Color</param>
		public void PolyLine(Vector4[] Nodes, uint Color)
		{
			int i, c = Nodes.Length;

			this.MoveTo(Nodes[0]);

			for (i = 1; i < c; i++)
				this.LineTo(Nodes[i], Color);
		}

		#endregion

		#region Scan Lines

		private void ScanLine(
			float sx0, float sy0, float sz0,
			float wx0, float wy0, float wz0,
			float sx1, float sz1,
			float wx1, float wy1, float wz1,
			Vector3 Normal, I3DShader Shader)
		{
			float Delta;

			if (sx1 < sx0)
			{
				Delta = sx0;
				sx0 = sx1;
				sx1 = Delta;

				Delta = sz0;
				sz0 = sz1;
				sz1 = Delta;

				Delta = wx0;
				wx0 = wx1;
				wx1 = Delta;

				Delta = wy0;
				wy0 = wy1;
				wy1 = Delta;

				Delta = wz0;
				wz0 = wz1;
				wz1 = Delta;
			}

			float sy1 = sy0;

			if (!this.ClipLine(
				ref sx0, ref sy0, ref sz0, ref wx0, ref wy0, ref wz0,
				ref sx1, ref sy1, ref sz1, ref wx1, ref wy1, ref wz1))
			{
				return;
			}

			if (sx0 == sx1)
			{
				if (wz0 < wz1)
				{
					this.Plot((int)(sx0 + 0.5f), (int)(sy0 + 0.5f), wz0,
						ToUInt(Shader.GetColor(wx0, wy0, wz0, Normal)));
				}
				else
				{
					this.Plot((int)(sx1 + 0.5f), (int)(sy1 + 0.5f), wz1,
						ToUInt(Shader.GetColor(wx1, wy1, wz1, Normal)));
				}
			}
			else
			{
				int isx0 = (int)(sx0 + 0.5f);
				int isx1 = (int)(sx1 + 0.5f);
				float dsx = 1 / (sx1 - sx0);
				float dwxdsx = (wx1 - wx0) * dsx;
				float dwydsx = (wy1 - wy0) * dsx;
				float dwzdsx = (wz1 - wz0) * dsx;
				int i = 0;
				int p = (int)(sy0 + 0.5f) * this.w + isx0;
				int p4 = p << 2;
				int c;
				SKColor cl;
				byte A;
				byte R2, G2, B2, A2;
				byte R3, G3, B3, A3;

				while (isx0 <= isx1)
				{
					this.xBuf[i] = wx0;
					this.yBuf[i] = wy0;
					this.zBuf[i] = wz0;
					this.normalBuf[i++] = Normal;
					wx0 += dwxdsx;
					wy0 += dwydsx;
					wz0 += dwzdsx;
					isx0++;
				}

				c = i;
				Shader.GetColors(this.xBuf, this.yBuf, this.zBuf, this.normalBuf, c, this.colorBuf);

				for (i = 0; i < c; i++)
				{
					wz0 = this.zBuf[i];

					if (wz0 > 0 && wz0 < this.zBuffer[p])
					{
						this.zBuffer[p++] = wz0;

						cl = this.colorBuf[i];

						if ((A = cl.Alpha) == 255)
						{
							this.pixels[p4++] = cl.Red;
							this.pixels[p4++] = cl.Green;
							this.pixels[p4++] = cl.Blue;
							this.pixels[p4++] = 255;
						}
						else
						{
							R2 = this.pixels[p4++];
							G2 = this.pixels[p4++];
							B2 = this.pixels[p4++];
							A2 = this.pixels[p4];

							if (A2 == 255)
							{
								R3 = (byte)(((cl.Red * A + R2 * (255 - A)) + 128) / 255);
								G3 = (byte)(((cl.Green * A + G2 * (255 - A)) + 128) / 255);
								B3 = (byte)(((cl.Blue * A + B2 * (255 - A)) + 128) / 255);
								A3 = 255;
							}
							else
							{
								R2 = (byte)((R2 * A2 + 128) / 255);
								G2 = (byte)((G2 * A2 + 128) / 255);
								B2 = (byte)((B2 * A2 + 128) / 255);

								R3 = (byte)(((cl.Red * A + R2 * (255 - A)) + 128) / 255);
								G3 = (byte)(((cl.Green * A + G2 * (255 - A)) + 128) / 255);
								B3 = (byte)(((cl.Blue * A + B2 * (255 - A)) + 128) / 255);
								A3 = (byte)(255 - (((255 - A) * (255 - A2) + 128) / 255));
							}

							this.pixels[p4--] = A3;
							this.pixels[p4--] = B3;
							this.pixels[p4--] = G3;
							this.pixels[p4] = R3;
							p4 += 4;
						}
					}
					else
					{
						p++;
						p4 += 4;
					}
				}
			}
		}

		#endregion

		#region Polygon

		internal static Vector3 ToVector3(Vector4 P)
		{
			if (P.W == 1 || P.W == 0)
				return new Vector3(P.X, P.Y, P.Z);

			float d = 1f / P.W;
			return new Vector3(P.X * d, P.Y * d, P.Z * d);
		}

		internal static Vector4 ToPoint(Vector3 P)
		{
			return new Vector4(P.X, P.Y, P.Z, 1);
		}

		internal static Vector4 ToVector(Vector3 P)
		{
			return new Vector4(P.X, P.Y, P.Z, 0);
		}

		private static Vector3 CalcNormal(Vector3 P0, Vector3 P1, Vector3 P2)
		{
			return Vector3.Normalize(Vector3.Cross(P1 - P0, P2 - P0));
		}

		private bool ClipTopBottom(
			ref float x0, ref float y0, ref float z0,
			ref float rx0, ref float ry0, ref float rz0,
			ref float x1, ref float y1, ref float z1,
			ref float rx1, ref float ry1, ref float rz1)
		{
			byte Mask0 = 0;
			byte Mask1 = 0;
			float Delta;

			if (y0 < 0)
				Mask0 |= 4;
			else if (y0 > this.hm1)
				Mask0 |= 8;

			if (y1 < 0)
				Mask1 |= 4;
			else if (y1 > this.hm1)
				Mask1 |= 8;

			if (Mask0 == 0 && Mask1 == 0)
				return true;

			if ((Mask0 & Mask1) != 0)
				return false;

			// Top edge:

			if ((Mask0 & 4) != 0)
			{
				Delta = y0 / (y1 - y0);    // Divisor is non-zero, or masks would have common bit.
				x0 -= (x1 - x0) * Delta;
				z0 -= (z1 - z0) * Delta;
				rx0 -= (rx1 - rx0) * Delta;
				ry0 -= (ry1 - ry0) * Delta;
				rz0 -= (rz1 - rz0) * Delta;
				y0 = 0;

				Mask0 &= 251;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 4) != 0)
			{
				Delta = y1 / (y0 - y1);    // Divisor is non-zero, or masks would have common bit.
				x1 -= (x0 - x1) * Delta;
				z1 -= (z0 - z1) * Delta;
				rx1 -= (rx0 - rx1) * Delta;
				ry1 -= (ry0 - ry1) * Delta;
				rz1 -= (rz0 - rz1) * Delta;
				y1 = 0;

				Mask1 &= 251;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			// Bottom edge:

			if ((Mask0 & 8) != 0)
			{
				Delta = (this.hm1 - y0) / (y1 - y0);    // Divisor is non-zero, or masks would have common bit.
				x0 += (x1 - x0) * Delta;
				z0 += (z1 - z0) * Delta;
				rx0 += (rx1 - rx0) * Delta;
				ry0 += (ry1 - ry0) * Delta;
				rz0 += (rz1 - rz0) * Delta;
				y0 = this.hm1;

				Mask0 &= 247;
				if (x0 < 0)
					Mask0 |= 1;
				else if (x0 > this.wm1)
					Mask0 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			if ((Mask1 & 8) != 0)
			{
				Delta = (this.hm1 - y1) / (y0 - y1);    // Divisor is non-zero, or masks would have common bit.
				x1 += (x0 - x1) * Delta;
				z1 += (z0 - z1) * Delta;
				rx1 += (rx0 - rx1) * Delta;
				ry1 += (ry0 - ry1) * Delta;
				rz1 += (rz0 - rz1) * Delta;
				y1 = this.hm1;

				Mask1 &= 247;
				if (x1 < 0)
					Mask1 |= 1;
				else if (x1 > this.wm1)
					Mask1 |= 2;

				if ((Mask0 & Mask1) != 0)
					return false;
			}

			return ((Mask0 | Mask1) == 0);
		}

		/// <summary>
		/// Draws a closed polygon.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Color">Color</param>
		/// <param name="TwoSided">If the polygon is two-sided.
		/// If true, <paramref name="Color"/> is used on both sides.
		/// If false, <paramref name="Color"/> is only used on the front side.
		/// Which side is the front side, is determined from the Normal vector
		/// and viewing position. The Normal vector is determined from the order 
		/// of the nodes defining the polygon.</param>
		public void Polygon(Vector4[] Nodes, SKColor Color, bool TwoSided)
		{
			this.Polygons(new Vector4[][] { Nodes }, new ConstantColor(Color), TwoSided);
		}

		/// <summary>
		/// Draws a closed polygon.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Shader">Shader.</param>
		/// <param name="TwoSided">If the polygon is two-sided.
		/// If true, <paramref name="Shader"/> is used on both sides.
		/// If false, <paramref name="Shader"/> is only used on the front side.
		/// Which side is the front side, is determined from the Normal vector
		/// and viewing position. The Normal vector is determined from the order 
		/// of the nodes defining the polygon.</param>
		public void Polygon(Vector4[] Nodes, I3DShader Shader, bool TwoSided)
		{
			this.Polygons(new Vector4[][] { Nodes }, Shader, TwoSided);
		}

		/// <summary>
		/// Draws a set of closed polygons. Interior polygons can be used to undraw the corresponding sections.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Color">Color</param>
		/// <param name="TwoSided">If the polygon is two-sided.
		/// If true, <paramref name="Color"/> is used on both sides.
		/// If false, <paramref name="Color"/> is only used on the front side.
		/// Which side is the front side, is determined from the Normal vector
		/// and viewing position. The Normal vector is determined from the order 
		/// of the nodes defining the polygon.</param>
		public void Polygons(Vector4[][] Nodes, SKColor Color, bool TwoSided)
		{
			this.Polygons(Nodes, new ConstantColor(Color), TwoSided);
		}

		/// <summary>
		/// Draws a set of closed polygons. Interior polygons can be used to undraw the corresponding sections.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="Shader">Shader.</param>
		/// <param name="TwoSided">If the polygon is two-sided.
		/// If true, <paramref name="Shader"/> is used on both sides.
		/// If false, <paramref name="Shader"/> is only used on the front side.
		/// Which side is the front side, is determined from the Normal vector
		/// and viewing position. The Normal vector is determined from the order 
		/// of the nodes defining the polygon.</param>
		public void Polygons(Vector4[][] Nodes, I3DShader Shader, bool TwoSided)
		{
			this.Polygons(Nodes, Shader, TwoSided ? Shader : null);
		}

		/// <summary>
		/// Draws a set of closed polygons. Interior polygons can be used to undraw the corresponding sections.
		/// </summary>
		/// <param name="Nodes">Nodes.</param>
		/// <param name="FrontShader">Front side Shader.</param>
		/// <param name="BackShader">Back side Shader.</param>
		public void Polygons(Vector4[][] Nodes, I3DShader FrontShader, I3DShader BackShader)
		{
			int j, d;
			int i, c;
			int MinY = 0;
			int MaxY = 0;
			int Y;
			Vector4 WP;
			Vector3 SP;
			Vector4[] v, vw;
			Vector3[] vs;
			bool First = true;

			FrontShader?.Transform(this);
			BackShader?.Transform(this);
			Nodes = (Vector4[][])Nodes.Clone();

			d = Nodes.Length;

			Vector4[][] World = new Vector4[d][];
			Vector3[][] Screen = new Vector3[d][];

			for (j = 0; j < d; j++)
			{
				v = Nodes[j];
				c = v.Length;

				if (c < 3)
					continue;

				vw = new Vector4[c];
				World[j] = vw;

				vs = new Vector3[c];
				Screen[j] = vs;

				for (i = 0; i < c; i++)
				{
					vw[i] = WP = this.ModelTransform(v[i]);
					vs[i] = SP = this.Project(WP);

					Y = (int)(SP.Y + 0.5f);

					if (First)
					{
						First = false;
						MinY = MaxY = Y;
					}
					else if (Y < MinY)
						MinY = Y;
					else if (Y > MaxY)
						MaxY = Y;
				}
			}

			if (MaxY < 0)
				return;
			else if (MinY < 0)
				MinY = 0;

			if (MinY >= this.h)
				return;
			else if (MaxY >= this.h)
				MaxY = this.hm1;

			int NrRecs = MaxY - MinY + 1;
			ScanLineRec[] Recs = new ScanLineRec[NrRecs];
			ScanLineRec Rec;
			Vector4 LastWorld;
			Vector4 CurrentWorld;
			Vector3 LastScreen;
			Vector3 CurrentScreen;
			Vector3 N;
			I3DShader Shader;
			float sx0, sy0, sz0;
			float sx1, sy1, sz1;
			float wx0, wy0, wz0;
			float wx1, wy1, wz1;
			float invdsy, dsxdsy, dszdsy;
			float dwxdsy, dwydsy, dwzdsy;
			int isy0, isy1;

			for (j = 0; j < d; j++)
			{
				vw = World[j];
				vs = Screen[j];
				c = vw.Length;

				if (c < 3)
					continue;

				//LastWorld = vw[c - 2];
				CurrentWorld = vw[c - 1];
				LastScreen = vs[c - 2];
				CurrentScreen = vs[c - 1];

				N = CalcNormal(ToVector3(vw[0]), ToVector3(vw[1]), ToVector3(CurrentWorld));

				if (Vector3.Dot(N, ToVector3(vw[0] - this.viewerPosition)) >= 0)
					Shader = FrontShader;
				else
					Shader = BackShader;

				if (Shader is null)
					continue;   // Culled

				sy0 = LastScreen.Y;
				sy1 = CurrentScreen.Y;

				isy0 = (int)(sy0 + 0.5f);
				isy1 = (int)(sy1 + 0.5f);

				int LastDir;
				int Dir = Math.Sign(isy1 - isy0);

				for (i = 0; i < c; i++)
				{
					LastWorld = CurrentWorld;
					CurrentWorld = vw[i];

					LastScreen = CurrentScreen;
					CurrentScreen = vs[i];

					sx0 = LastScreen.X;
					sy0 = LastScreen.Y;
					sz0 = LastScreen.Z;

					sx1 = CurrentScreen.X;
					sy1 = CurrentScreen.Y;
					sz1 = CurrentScreen.Z;

					wx0 = LastWorld.X;
					wy0 = LastWorld.Y;
					wz0 = LastWorld.Z;

					wx1 = CurrentWorld.X;
					wy1 = CurrentWorld.Y;
					wz1 = CurrentWorld.Z;

					if (!this.ClipTopBottom(
						ref sx0, ref sy0, ref sz0,
						ref wx0, ref wy0, ref wz0,
						ref sx1, ref sy1, ref sz1,
						ref wx1, ref wy1, ref wz1))
					{
						continue;
					}

					isy0 = (int)(sy0 + 0.5f);
					isy1 = (int)(sy1 + 0.5f);

					LastDir = Dir;
					Dir = Math.Sign(isy1 - isy0);

					if (Dir == 0)
						continue;

					invdsy = 1 / (sy1 - sy0);
					dsxdsy = (sx1 - sx0) * invdsy;
					dszdsy = (sz1 - sz0) * invdsy;
					dwxdsy = (wx1 - wx0) * invdsy;
					dwydsy = (wy1 - wy0) * invdsy;
					dwzdsy = (wz1 - wz0) * invdsy;

					if (Dir > 0)
					{
						if (Dir == LastDir)
						{
							isy0++;
							sx0 += dsxdsy;
							sz0 += dszdsy;
							wx0 += dwxdsy;
							wy0 += dwydsy;
							wz0 += dwzdsy;
						}

						while (isy0 <= isy1)
						{
							this.AddNode(Recs, MinY, sx0, isy0, sz0, wx0, wy0, wz0, N, Shader);

							isy0++;
							sx0 += dsxdsy;
							sz0 += dszdsy;
							wx0 += dwxdsy;
							wy0 += dwydsy;
							wz0 += dwzdsy;
						}
					}
					else
					{
						if (Dir == LastDir)
						{
							isy0--;
							sx0 -= dsxdsy;
							sz0 -= dszdsy;
							wx0 -= dwxdsy;
							wy0 -= dwydsy;
							wz0 -= dwzdsy;
						}

						while (isy0 >= isy1)
						{
							this.AddNode(Recs, MinY, sx0, isy0, sz0, wx0, wy0, wz0, N, Shader);

							isy0--;
							sx0 -= dsxdsy;
							sz0 -= dszdsy;
							wx0 -= dwxdsy;
							wy0 -= dwydsy;
							wz0 -= dwzdsy;
						}
					}
				}
			}

			for (i = 0; i < NrRecs; i++)
			{
				Rec = Recs[i];
				if (Rec is null)
					continue;

				Y = i + MinY;

				if (!(Rec.segments is null))
				{
					First = true;
					Shader = null;

					sx0 = sz0 = wx0 = wy0 = wz0 = 0;
					foreach (ScanLineSegment Rec2 in Rec.segments)
					{
						if (First)
						{
							First = false;
							sx0 = Rec2.sx;
							sz0 = Rec2.sz;
							wx0 = Rec2.wx;
							wy0 = Rec2.wy;
							wz0 = Rec2.wz;
							Shader = Rec2.shader;
						}
						else
						{
							Shader = Rec2.shader;
							this.ScanLine(sx0, Y, sz0, wx0, wy0, wz0,
								Rec2.sx, Rec2.sz, Rec2.wx, Rec2.wy, Rec2.wz,
								Rec.n, Shader);

							First = true;
						}
					}

					if (!First)
					{
						this.Plot((int)(sx0 + 0.5f), Y, sz0,
							ToUInt(Shader.GetColor(wx0, wy0, wz0, Rec.n)));
					}
				}
				else if (Rec.has2)
				{
					this.ScanLine(Rec.sx0, Y, Rec.sz0, Rec.wx0, Rec.wy0, Rec.wz0,
						Rec.sx1, Rec.sz1, Rec.wx1, Rec.wy1, Rec.wz1, Rec.n, Rec.shader);
				}
				else
				{
					this.Plot((int)(Rec.sx0 + 0.5f), Y, Rec.sz0, 
						ToUInt(Rec.shader.GetColor(Rec.wx0, Rec.wy0, Rec.wz0, Rec.n)));
				}
			}
		}

		private void AddNode(ScanLineRec[] Records, int MinY, float sx, float sy, float sz,
			float wx, float wy, float wz, Vector3 N, I3DShader Shader)
		{
			int i = (int)(sy + 0.5f) - MinY;
			ScanLineRec Rec = Records[i];

			if (Rec is null)
			{
				Records[i] = new ScanLineRec()
				{
					sx0 = sx,
					sz0 = sz,
					wx0 = wx,
					wy0 = wy,
					wz0 = wz,
					has2 = false,
					n = N,
					shader = Shader
				};
			}
			else if (!Rec.has2)
			{
				if (sx < Rec.sx0)
				{
					Rec.sx1 = Rec.sx0;
					Rec.sz1 = Rec.sz0;
					Rec.wx1 = Rec.wx0;
					Rec.wy1 = Rec.wy0;
					Rec.wz1 = Rec.wz0;
					Rec.sx0 = sx;
					Rec.sz0 = sz;
					Rec.wx0 = wx;
					Rec.wy0 = wy;
					Rec.wz0 = wz;
				}
				else
				{
					Rec.sx1 = sx;
					Rec.sz1 = sz;
					Rec.wx1 = wx;
					Rec.wy1 = wy;
					Rec.wz1 = wz;
				}

				Rec.shader = Shader;
				Rec.has2 = true;
			}
			else
			{
				if (Rec.segments is null)
				{
					Rec.segments = new LinkedList<ScanLineSegment>();
					Rec.segments.AddLast(new ScanLineSegment()
					{
						sx = Rec.sx0,
						sz = Rec.sz0,
						wx = Rec.wx0,
						wy = Rec.wy0,
						wz = Rec.wz0,
						shader = Rec.shader
					});
					Rec.segments.AddLast(new ScanLineSegment()
					{
						sx = Rec.sx1,
						sz = Rec.sz1,
						wx = Rec.wx1,
						wy = Rec.wy1,
						wz = Rec.wz1,
						shader = Rec.shader
					});
				}

				LinkedListNode<ScanLineSegment> Loop = Rec.segments.First;
				LinkedListNode<ScanLineSegment> Prev = null;

				while (!(Loop is null) && Loop.Value.sx < sx)
				{
					Prev = Loop;
					Loop = Loop.Next;
				}

				ScanLineSegment Segment = new ScanLineSegment()
				{
					sx = sx,
					sz = sz,
					wx = wx,
					wy = wy,
					wz = wz,
					shader = Rec.shader
				};

				if (Loop is null)
					Rec.segments.AddLast(Segment);
				else if (Prev is null)
					Rec.segments.AddFirst(Segment);
				else
					Rec.segments.AddAfter(Prev, Segment);
			}
		}

		private class ScanLineRec
		{
			public float sx0;
			public float sz0;
			public float wx0;
			public float wy0;
			public float wz0;
			public float sx1;
			public float sz1;
			public float wx1;
			public float wy1;
			public float wz1;
			public bool has2;
			public LinkedList<ScanLineSegment> segments;
			public Vector3 n;
			public I3DShader shader;
		}

		private class ScanLineSegment
		{
			public float sx;
			public float sz;
			public float wx;
			public float wy;
			public float wz;
			public I3DShader shader;
		}

		#endregion

		#region Text

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, float TextSize, SKColor Color)
		{
			this.Text(Text, Start, FontFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
				SKFontStyleSlant.Upright, TextSize, Color);
		}

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="Weight">Font weight.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, SKFontStyleWeight Weight,
			float TextSize, SKColor Color)
		{
			this.Text(Text, Start, FontFamily, Weight, SKFontStyleWidth.Normal,
				SKFontStyleSlant.Upright, TextSize, Color);
		}

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="Weight">Font weight.</param>
		/// <param name="Width">Font width.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, SKFontStyleWeight Weight,
			SKFontStyleWidth Width, float TextSize, SKColor Color)
		{
			this.Text(Text, Start, FontFamily, Weight, Width, SKFontStyleSlant.Upright,
				TextSize, Color);
		}

		/// <summary>
		/// Draws text on the canvas.
		/// </summary>
		/// <param name="Text">Text to draw.</param>
		/// <param name="Start">Start position.</param>
		/// <param name="FontFamily">Font family.</param>
		/// <param name="Weight">Font weight.</param>
		/// <param name="Width">Font width.</param>
		/// <param name="Slant">Font slant.</param>
		/// <param name="TextSize">Text size.</param>
		/// <param name="Color">Text color.</param>
		public void Text(string Text, Vector4 Start, string FontFamily, SKFontStyleWeight Weight,
			SKFontStyleWidth Width, SKFontStyleSlant Slant, float TextSize, SKColor Color)
		{
			SKPaint Paint = null;
			SKPath Path = null;
			SKPath Simple = null;
			SKPath.Iterator e = null;
			SKPoint[] Points = new SKPoint[4];
			SKPathVerb Verb;

			try
			{
				Paint = new SKPaint()
				{
					Typeface = SKTypeface.FromFamilyName(FontFamily, Weight, Width, Slant),
					TextSize = TextSize
				};

				Path = Paint.GetTextPath(Text, 0, 0);
				//Simple = Path.Simplify();

				e = Path.CreateIterator(false);

				List<Vector4> P = new List<Vector4>();
				List<Vector4[]> v = new List<Vector4[]>();
				float MaxX = 0;
				float X, Y;
				float x0, x1, x2, x3;
				float y0, y1, y2, y3;
				float dx, dy, t, w, d, t2, w2, t3, w3, weight;
				int i, c;

				while ((Verb = e.Next(Points)) != SKPathVerb.Done)
				{
					switch (Verb)
					{
						case SKPathVerb.Close:
							if ((c = P.Count) > 1 && P[0] == P[c - 1])
								P.RemoveAt(c - 1);

							v.Add(P.ToArray());
							P.Clear();
							break;

						case SKPathVerb.Move:
							X = Points[0].X;
							if (X > MaxX)
							{
								if (v.Count > 0)
								{
									this.Polygons(v.ToArray(), Color, true);
									v.Clear();
								}

								MaxX = X;
							}

							if (P.Count > 0)
							{
								if ((c = P.Count) > 1 && P[0] == P[c - 1])
									P.RemoveAt(c - 1);

								v.Add(P.ToArray());
								P.Clear();
							}

							P.Add(new Vector4(Start.X + X, Start.Y - Points[0].Y, Start.Z, 1));
							break;

						case SKPathVerb.Line:
							X = Points[1].X;
							if (X > MaxX)
								MaxX = X;

							P.Add(new Vector4(Start.X + X, Start.Y - Points[1].Y, Start.Z, 1));
							break;

						case SKPathVerb.Quad:
							x0 = Points[0].X;
							y0 = Points[0].Y;
							if (x0 > MaxX)
								MaxX = x0;

							x1 = Points[1].X;
							y1 = Points[1].Y;
							if (x1 > MaxX)
								MaxX = x1;

							x2 = Points[2].X;
							y2 = Points[2].Y;
							if (x2 > MaxX)
								MaxX = x2;

							dx = x2 - x0;
							dy = y2 - y0;

							c = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy) / 5);
							for (i = 1; i <= c; i++)
							{
								t = ((float)i) / c;
								w = 1 - t;

								t2 = t * t;
								w2 = w * w;

								X = w2 * x0 + 2 * t * w * x1 + t2 * x2;
								Y = w2 * y0 + 2 * t * w * y1 + t2 * y2;

								P.Add(new Vector4(Start.X + X, Start.Y - Y, Start.Z, 1));
							}
							break;

						case SKPathVerb.Conic:
							x0 = Points[0].X;
							y0 = Points[0].Y;
							if (x0 > MaxX)
								MaxX = x0;

							x1 = Points[1].X;
							y1 = Points[1].Y;
							if (x1 > MaxX)
								MaxX = x1;

							x2 = Points[2].X;
							y2 = Points[2].Y;
							if (x2 > MaxX)
								MaxX = x2;

							dx = x2 - x0;
							dy = y2 - y0;

							weight = e.ConicWeight();

							c = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy) / 5);
							for (i = 1; i <= c; i++)
							{
								t = ((float)i) / c;
								w = 1 - t;

								t2 = t * t;
								w2 = w * w;

								d = 1.0f / (w2 + 2 * weight * t * w + t2);
								X = (w2 * x0 + 2 * weight * t * w * x1 + t2 * x2) * d;
								Y = (w2 * y0 + 2 * weight * t * w * y1 + t2 * y2) * d;

								P.Add(new Vector4(Start.X + X, Start.Y - Y, Start.Z, 1));
							}
							break;

						case SKPathVerb.Cubic:
							x0 = Points[0].X;
							y0 = Points[0].Y;
							if (x0 > MaxX)
								MaxX = x0;

							x1 = Points[1].X;
							y1 = Points[1].Y;
							if (x1 > MaxX)
								MaxX = x1;

							x2 = Points[2].X;
							y2 = Points[2].Y;
							if (x2 > MaxX)
								MaxX = x2;

							x3 = Points[3].X;
							y3 = Points[3].Y;
							if (x3 > MaxX)
								MaxX = x3;

							dx = x3 - x0;
							dy = y3 - y0;

							c = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy) / 5);
							for (i = 1; i <= c; i++)
							{
								t = ((float)i) / c;
								w = 1 - t;

								t2 = t * t;
								w2 = w * w;

								t3 = t2 * t;
								w3 = w2 * w;

								X = w3 * x0 + 3 * t * w2 * x1 + 3 * t2 * w * x2 + t3 * x3;
								Y = w3 * y0 + 3 * t * w2 * y1 + 3 * t2 * w * y2 + t3 * y3;

								P.Add(new Vector4(Start.X + X, Start.Y - Y, Start.Z, 1));
							}
							break;
					}
				}

				if (v.Count > 0)
					this.Polygons(v.ToArray(), Color, true);
			}
			finally
			{
				Paint?.Dispose();
				Path?.Dispose();
				Simple?.Dispose();
				e?.Dispose();
			}
		}

		#endregion

		#region Box

		/// <summary>
		/// Draws a box, with sides parallell to the x, y and z axis.
		/// </summary>
		/// <param name="Corner1">First corner</param>
		/// <param name="Corner2">Second corner</param>
		/// <param name="Shader">Shader.</param>
		public void Box(Vector4 Corner1, Vector4 Corner2, I3DShader Shader)
		{
			this.Box(ToVector3(Corner1), ToVector3(Corner2), Shader);
		}

		/// <summary>
		/// Draws a box, with sides parallell to the x, y and z axis.
		/// </summary>
		/// <param name="Corner1">First corner</param>
		/// <param name="Corner2">Second corner</param>
		/// <param name="Shader">Shader.</param>
		public void Box(Vector3 Corner1, Vector3 Corner2, I3DShader Shader)
		{
			this.Box(Corner1.X, Corner1.Y, Corner1.Z, Corner2.X, Corner2.Y, Corner2.Z, Shader);
		}

		/// <summary>
		/// Draws a box, with sides parallell to the x, y and z axis.
		/// </summary>
		/// <param name="x1">X-axis of first corner.</param>
		/// <param name="y1">Y-axis of first corner.</param>
		/// <param name="z1">Z-axis of first corner.</param>
		/// <param name="x2">X-axis of second corner.</param>
		/// <param name="y2">Y-axis of second corner.</param>
		/// <param name="z2">Z-axis of second corner.</param>
		/// <param name="Shader">Shader.</param>
		public void Box(float x1, float y1, float z1, float x2, float y2, float z2, I3DShader Shader)
		{
			Vector4 P0 = new Vector4(x1, y1, z1, 1);
			Vector4 P1 = new Vector4(x1, y1, z2, 1);
			Vector4 P2 = new Vector4(x2, y1, z2, 1);
			Vector4 P3 = new Vector4(x2, y1, z1, 1);
			Vector4 P4 = new Vector4(x1, y2, z1, 1);
			Vector4 P5 = new Vector4(x1, y2, z2, 1);
			Vector4 P6 = new Vector4(x2, y2, z2, 1);
			Vector4 P7 = new Vector4(x2, y2, z1, 1);

			this.Polygon(new Vector4[] { P0, P1, P2, P3 }, Shader, false);
			this.Polygon(new Vector4[] { P7, P6, P5, P4 }, Shader, false);
			this.Polygon(new Vector4[] { P5, P6, P2, P1 }, Shader, false);
			this.Polygon(new Vector4[] { P4, P5, P1, P0 }, Shader, false);
			this.Polygon(new Vector4[] { P6, P7, P3, P2 }, Shader, false);
			this.Polygon(new Vector4[] { P0, P3, P7, P4 }, Shader, false);
		}

		#endregion

	}
}
