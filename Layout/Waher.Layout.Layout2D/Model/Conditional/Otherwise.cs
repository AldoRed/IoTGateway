﻿namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Layout to show, if none of the cases apply.
	/// </summary>
	public class Otherwise : LayoutContainer
	{
		/// <summary>
		/// Layout to show, if none of the cases apply.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Otherwise(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Otherwise";

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Otherwise(Document, Parent);
		}
	}
}
