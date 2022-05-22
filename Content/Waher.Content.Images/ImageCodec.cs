﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;
using Waher.Runtime.Inventory;
using System.Threading.Tasks;

namespace Waher.Content.Images
{
	/// <summary>
	/// Image encoder/decoder.
	/// </summary>
	public class ImageCodec : IContentDecoder, IContentEncoder
	{
		/// <summary>
		/// Image encoder/decoder.
		/// </summary>
		public ImageCodec()
		{
		}

		/// <summary>
		/// Image content types.
		/// </summary>
		public static readonly string[] ImageContentTypes = new string[] 
		{
			"image/webp", 
			"image/png", 
			"image/bmp",
			"image/gif", 
			"image/jpeg", 
			"image/tiff", 
			"image/x-wmf", 
			"image/x-emf", 
			"image/x-icon"
		};

		/// <summary>
		/// Image content types.
		/// </summary>
		public static readonly string[] ImageFileExtensions = new string[] 
		{
			"webp", 
			"png", 
			"bmp",
			"gif", 
			"jpg", 
			"jpeg", 
			"tif", 
			"tiff", 
			"wmf", 
			"emf", 
			"ico"
		};

		/// <summary>
		/// Supported content types.
		/// </summary>
		public string[] ContentTypes => ImageContentTypes;

		/// <summary>
		/// Supported file extensions.
		/// </summary>
		public string[] FileExtensions => ImageFileExtensions;

		/// <summary>
		/// If the decoder decodes an object with a given content type.
		/// </summary>
		/// <param name="ContentType">Content type to decode.</param>
		/// <param name="Grade">How well the decoder decodes the object.</param>
		/// <returns>If the decoder can decode an object with the given type.</returns>
		public bool Decodes(string ContentType, out Grade Grade)
		{
			if (Array.IndexOf<string>(ImageContentTypes, ContentType) >= 0)
			{
				Grade = Grade.Excellent;
				return true;
			}
			else if (ContentType.StartsWith("image/"))
			{
				Grade = Grade.Barely;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Decodes an object.
		/// </summary>
		/// <param name="ContentType">Internet Content Type.</param>
		/// <param name="Data">Encoded object.</param>
		/// <param name="Encoding">Any encoding specified. Can be null if no encoding specified.</param>
		/// <param name="Fields">Any content-type related fields and their corresponding values.</param>
		///	<param name="BaseUri">Base URI, if any. If not available, value is null.</param>
		/// <returns>Decoded object.</returns>
		/// <exception cref="ArgumentException">If the object cannot be decoded.</exception>
		public Task<object> DecodeAsync(string ContentType, byte[] Data, Encoding Encoding, KeyValuePair<string, string>[] Fields, Uri BaseUri)
		{
			SKBitmap Bitmap = SKBitmap.Decode(Data);
			return Task.FromResult<object>(SKImage.FromBitmap(Bitmap));
		}

		/// <summary>
		/// If the encoder encodes a given object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Grade">How well the encoder encodes the object.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>If the encoder can encode the given object.</returns>
		public bool Encodes(object Object, out Grade Grade, params string[] AcceptedContentTypes)
		{
			if ((Object is SKImage || Object is SKBitmap) && InternetContent.IsAccepted(ContentTypes, AcceptedContentTypes))
			{
				Grade = Grade.Ok;
				return true;
			}
			else
			{
				Grade = Grade.NotAtAll;
				return false;
			}
		}

		/// <summary>
		/// Encodes an object.
		/// </summary>
		/// <param name="Object">Object to encode.</param>
		/// <param name="Encoding">Desired encoding of text. Can be null if no desired encoding is speified.</param>
		/// <param name="AcceptedContentTypes">Optional array of accepted content types. If array is empty, all content types are accepted.</param>
		/// <returns>Encoded object, as well as Content Type of encoding. Includes information about any text encodings used.</returns>
		/// <exception cref="ArgumentException">If the object cannot be encoded.</exception>
		public Task<KeyValuePair<byte[], string>> EncodeAsync(object Object, Encoding Encoding, params string[] AcceptedContentTypes)
		{
			SKData Data;
			bool Dispose = false;
			string ContentType;
			byte[] Bin;

			if (!(Object is SKImage Image))
			{
				if (Object is SKBitmap Bitmap)
				{
					Image = SKImage.FromBitmap(Bitmap);
					Dispose = true;
				}
				else
					throw new ArgumentException("Object not an image derived from SkiaSharp.SKImage or SkiaSharp.SKBitmap.", nameof(Object));
			}

			if (InternetContent.IsAccepted("image/webp", AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Webp, 100);
				ContentType = "image/webp";
			}
			else if (InternetContent.IsAccepted("image/png", AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Png, 100);
				ContentType = "image/png";
			}
			else if (InternetContent.IsAccepted("image/bmp", AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Bmp, 100);
				ContentType = "image/bmp";
			}
			else if (InternetContent.IsAccepted("image/jpeg", AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Jpeg, 90);
				ContentType = "image/jpeg";
			}
			else if (InternetContent.IsAccepted("image/gif", AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Gif, 100);
				ContentType = "image/gif";
			}
			else if (InternetContent.IsAccepted("image/x-icon", AcceptedContentTypes))
			{
				Data = Image.Encode(SKEncodedImageFormat.Ico, 100);
				ContentType = "image/x-icon";
			}
			else
				throw new ArgumentException("Unable to encode object, or content type not accepted.", nameof(Object));

			Bin = Data.ToArray();
			Data.Dispose();

			if (Dispose)
				Image.Dispose();

			return Task.FromResult<KeyValuePair<byte[], string>>(new KeyValuePair<byte[], string>(Bin, ContentType));
		}

		/// <summary>
		/// Tries to get the content type of an item, given its file extension.
		/// </summary>
		/// <param name="FileExtension">File extension.</param>
		/// <param name="ContentType">Content type.</param>
		/// <returns>If the extension was recognized.</returns>
		public bool TryGetContentType(string FileExtension, out string ContentType)
		{
			switch (FileExtension.ToLower())
			{
				case "png":
					ContentType = "image/png";
					return true;

				case "bmp":
					ContentType = "image/bmp";
					return true;

				case "gif":
					ContentType = "image/gif";
					return true;

				case "jpg":
				case "jpeg":
					ContentType = "image/jpeg";
					return true;

				case "webp":
					ContentType = "image/webp";
					return true;

				case "tif":
				case "tiff":
					ContentType = "image/tiff";
					return true;

				case "wmf":
					ContentType = "image/x-wmf";
					return true;

				case "emf":
					ContentType = "image/x-emf";
					return true;
	
				case "ico":
					ContentType = "image/x-icon";
					return true;

				case "svg":
					ContentType = "image/svg+xml";
					return true;

				default:
					ContentType = string.Empty;
					return false;
			}
		}
	
		/// <summary>
		/// Tries to get the file extension of an item, given its Content-Type.
		/// </summary>
		/// <param name="ContentType">Content type.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>If the Content-Type was recognized.</returns>
		public bool TryGetFileExtension(string ContentType, out string FileExtension)
		{
			switch (ContentType.ToLower())
			{
				case "image/png":
					FileExtension = "png";
					return true;

				case "image/bmp":
					FileExtension = "bmp";
					return true;

				case "image/gif":
					FileExtension = "gif";
					return true;

				case "image/jpeg":
					FileExtension = "jpg";
					return true;

				case "image/webp":
					FileExtension = "webp";
					return true;

				case "image/tiff":
					FileExtension = "tif";
					return true;

				case "image/x-wmf":
					FileExtension = "wmf";
					return true;

				case "image/x-emf":
					FileExtension = "emf";
					return true;

				case "image/x-icon":
					FileExtension = "ico";
					return true;

				case "image/svg+xml":
					FileExtension = "svg";
					return true;

				default:
					FileExtension = string.Empty;
					return false;
			}
		}

	}
}
