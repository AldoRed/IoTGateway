﻿using System;

namespace Waher.Layout.Layout2D.Model
{
	/// <summary>
	/// Interface for layout elements that generate children dynamically.
	/// </summary>
	public interface IDynamicChildren
	{
		/// <summary>
		/// Dynamic array of children
		/// </summary>
		ILayoutElement[] DynamicChildren
		{ 
			get; 
		}
	}
}
