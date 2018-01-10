using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Lemur.Xml {

	public class XMLNode {

		/// <summary>
		/// Next Xml sibling.
		/// </summary>
		public XMLNode Next { get => next; set => next = value; }
		private XMLNode next;

		/// <summary>
		/// Previous Xml sibling.
		/// </summary>
		public XMLNode Prev { get => prev; set => prev = value; }
		private XMLNode prev;

		private XMLList _children;
		public XMLList children {
			get {
				return this._children;
			}
		}

		/// <summary>
		/// Text contained as node content.
		/// </summary>
		public string NodeText {
			get {
				return this._nodeText;
			}
			set {
				this._nodeText = value;
				this.IsTextNode = true;
			}
		}
		private string _nodeText;

		private bool _isTextNode;

		/// <summary>
		/// Returns true if the node is a text node. Text nodes do not
		/// have any child nodes.
		/// </summary>
		public bool IsTextNode {
			get {
				return this._isTextNode;
			}
			set {
				this._isTextNode = value;
			}
		}

		/// <summary>
		/// Attributes stored in this xml node.
		/// </summary>
		private Dictionary< String, String > _attributes;

		public string GetAttribute( string attr ) {

			string attrValue;

			if ( this._attributes.TryGetValue( attr, out attrValue ) ) {
				return attrValue;
			}

			return null;

		} //

		public bool TryGetBool( string attr, out bool result ) {

			string attrValue;

			if ( this._attributes.TryGetValue( attr, out attrValue ) ) {
				result = Convert.ToBoolean( attrValue );
				return true;
			}
			result = false;
			return false;

		} //

		public int GetIntAttribute( string attr ) {

			string attrValue;
			
			if ( this._attributes.TryGetValue( attr, out attrValue ) ) {
				return Convert.ToInt32( attrValue );
			}
			
			return 0;

		} //

		public float GetFloatAttribute( string attr ) {

			string attrValue;

			if ( this._attributes.TryGetValue( attr, out attrValue ) ) {
				return (float)Convert.ToDouble( attrValue );
			}

			return 0;

		} //

		public void SetAttribute( string attr, string value ) {

			this._attributes[attr] = value;

		} //


		public void SetAttribute( string attr, int value ) {
			
			this._attributes[attr] = value.ToString();
			
		} //

		public bool TryGetAttribute( string attr, out string attrValue ) {

			if ( this._attributes.TryGetValue( attr, out attrValue ) ) {
				return true;
			}

			attrValue = null;
			return false;

		}

		public XMLNode FirstChild {
			get {
				return this._children.First;
			}
		}

		public bool HasChildren() {

			return this.children.HasNodes();
		}

		public int NumChildren {
	
			get {
				return this.children.CountNodes();
			}

		}

		public string NodeName {
			get;
			set;
		}

		public XMLNode() {

			this._attributes = new Dictionary<string, string>();
			this._children = new XMLList();

		} //

		public XMLNode( string nodeName ) {

			this.NodeName = nodeName;
			this._attributes = new Dictionary<string, string>();
			this._children = new XMLList();

		} //

		/**
		 * create an xml node with the given inner text.
		 */
		public XMLNode( string nodeName, string nodeText ) : this( nodeName ) {

			this._nodeText = nodeText;

		} //

		public void AddChild( XMLNode node ) {

			this.children.AddNode( node );

		} //

		/**
		 * when storing a single XMLNode for later use, remove it from its parent first
		 * so the siblings aren't retained in linked-list references.
		 */
		public void RemoveChild( XMLNode node ) {

			this.children.RemoveNode( node );

		} //

		// attribute indexer. don't use on text nodes. idiots.
		public string this[ string attribute ] {

			get {

				string value;

				if ( this._attributes.TryGetValue( attribute, out value ) ) {
					return value;
				}

				return null;

			} //

			set {
				this._attributes[ attribute ] = value;
			}

		} //

		public void childString() {
		} //

		public static implicit operator string( XMLNode node ) {
			return node.ToString();
		}

		override public string ToString() {

			if ( this._nodeText != null && this._nodeText != String.Empty ) {

				StringBuilder result = new StringBuilder();
				result.Append( "<" + this.NodeName );
				this.AttributeString( result );
				result.Append( ">" );

				result.Append( this._nodeText );

				result.Append( "</" + this.NodeName + ">" );

				return result.ToString();

			} else if ( this.HasChildren() ) {

				StringBuilder result = new StringBuilder();
				result.Append( "<" + this.NodeName );
				this.AttributeString( result );
				result.Append( ">" );

				for ( XMLNode node = this.children.First; node != null; node = node.Next ) {

					result.Append( node.ToString() );

				} //

				result.Append( "</" + this.NodeName + ">" );

				return result.ToString();

			} else {

				// need to add attributes, sub-nodes, etc.
				return "<" + this.NodeName + this.AttributeString() + "/>";

			}

		} // ToString()

		private string AttributeString() {

			StringBuilder list = new StringBuilder( " " );

			foreach( var item in this._attributes ) {

				list.Append( item.Key + "=\"" + item.Value + "\" " );

			} //

			return list.ToString();

		} //

		private void AttributeString( StringBuilder list  ) {

			foreach ( var item in this._attributes ) {

				list.Append( " " + item.Key + "=\"" + item.Value + "\" " );

			} //

		} //


	} // class

}