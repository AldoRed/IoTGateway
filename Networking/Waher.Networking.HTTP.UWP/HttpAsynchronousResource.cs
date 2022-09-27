﻿namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Base class for all asynchronous HTTP resources.
	/// An asynchronous resource responds outside of the method handler,
	/// from a separate thread or task. The application is responsible
	/// for returning the response and disposing of the response object
	/// when done.
	/// </summary>
	public abstract class HttpAsynchronousResource : HttpResource
	{
		/// <summary>
		/// Base class for all asynchronous HTTP resources.
		/// An asynchronous resource responds outside of the method handler,
		/// from a separate thread or task. The application is responsible
		/// for returning the response and disposing of the response object
		/// when done.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		public HttpAsynchronousResource(string ResourceName)
			: base(ResourceName)
		{
		}

		/// <summary>
		/// If the resource is synchronous (i.e. returns a response in the method handler), or if it is asynchronous
		/// (i.e. sends the response from another thread).
		/// </summary>
		public override bool Synchronous
		{
			get { return false; }
		}

	}
}
