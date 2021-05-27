﻿using System;
using System.Collections.Generic;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script.Abstraction.Sets
{
	/// <summary>
	/// Basic interface for ordered sets.
	/// </summary>
	public interface IOrderedSet : IComparer<IElement>
	{
	}
}
