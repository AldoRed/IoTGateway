﻿using System;
using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Orders
{
	/// <summary>
	/// Orders entries based on relevance.
	/// </summary>
	public class RelevanceOrder : IComparer<MatchInformation>
	{
		/// <summary>
		/// Orders entries based on relevance.
		/// </summary>
		public RelevanceOrder()
		{
		}

		/// <summary>
		/// <see cref="IComparer{MatchInformation}.Compare"/>
		/// </summary>
		public int Compare(MatchInformation x, MatchInformation y)
		{
			int i = (int)(y.NrDistinctTokens - x.NrDistinctTokens);
			if (i != 0)
				return i;

			long l = (long)(y.NrTokens - x.NrTokens);

			if (l < 0)
				return -1;
			else if (l > 0)
				return 1;

			return y.Timestamp.CompareTo(x.Timestamp);
		}
	}
}
