using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemur.Xml {

	/**
	 * WARNING:  Adding/removing nodes while traversing an XMLList may cause errors.
	 **/

	public class XMLList {

		private XMLNode _first;
		private XMLNode _last;

		/// <summary>
		/// First XmlNode in the list.
		/// </summary>
		public XMLNode First {

			get {
				return this._first;
			}

			set {

				if ( this._first == null ) {
					this._first = this._last = value;
				} else {

					value.Next = this._first;
					value.Prev = null;
					value.Next.Prev = value;

					this._first = value;

				} //

			}

		} //

		public int CountNodes() {

			int n = 0;

			for ( XMLNode node = this._first; node != null; node = node.Next ) {

				n++;

			} //

			return n;

		} //

		public bool HasNodes() {

			return ( this._first != null );

		} //

		public XMLNode Last {

			get {
				return this._last;
			}

			set {
				// add child adds to the end of the list.
				this.AddNode( value );
			}

		} //

		public XMLList() {

			this._first = null;
			this._last = null;

		} //

		/**
		 * This only works if node is actually in this list.
		 */
		public void RemoveNode( XMLNode node ) {

			if ( node == this._first ) {

				if ( node.Next != null ) {

					this._first = node.Next;
					node.Next.Prev = null;
					node.Next = null;

				} else {
					this._first = this._last = null;
				} //

			} else if ( node == this._last ) {

				this._last = node.Prev;

				node.Prev.Next = null;
				node.Prev = null;

			} else {

				// not first or last, so it must have both prev and next.
				node.Prev.Next = node.Next;
				node.Next.Prev = node.Prev;

				node.Next = node.Prev = null;

			} //

		} //

		public void AddNode( XMLNode node ) {

			if ( this._first == null ) {

				this._first = this._last = node;

			} else {

				node.Prev = this._last;
				node.Next = null;
				node.Prev.Next = node;

				this._last = node;

			} //

		} // addChild()

	} //

} //
