﻿using System;
using Waher.Runtime.Inventory;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;

namespace Waher.Script.Objects
{
    /// <summary>
    /// Namespace.
    /// </summary>
    public sealed class Namespace : Element
    {
        private static readonly Namespaces associatedSet = new Namespaces();

        private readonly string value;

        /// <summary>
        /// Namespace value.
        /// </summary>
        /// <param name="Value">Namespace value.</param>
        public Namespace(string Value)
        {
            this.value = Value;
        }

        /// <summary>
        /// Namespace.
        /// </summary>
        public string Value
        {
            get { return this.value; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.value;
        }

        /// <summary>
        /// Associated Set.
        /// </summary>
        public override ISet AssociatedSet
        {
            get { return associatedSet; }
        }

        /// <summary>
        /// Associated object value.
        /// </summary>
        public override object AssociatedObjectValue
        {
            get { return this; }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
			if (!(obj is Namespace E))
				return false;
			else
				return this.value == E.value;
		}

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// Access to types and subnamespaces in the current namespace.
        /// </summary>
        /// <param name="Name">Name of local element.</param>
        /// <returns>Local element reference.</returns>
        public IElement this[string Name]
        {
            get
            {
                string FullName = this.value + "." + Name;
                Type T;

                T = Types.GetType(FullName);
                if (!(T is null))
                    return new TypeValue(T);

                if (Types.IsSubNamespace(this.value, Name))
                    return new Namespace(FullName);

                throw new ScriptException("No namespace or type named '" + Name + "'.");
            }
        }

    }
}
