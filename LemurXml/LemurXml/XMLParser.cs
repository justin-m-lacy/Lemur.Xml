using System;
using System.Collections.Generic;
using System.Text;

namespace Lemur.Xml {

	public class XMLParser {

		private const char EOF_CHAR = '\u0026';
		private const char START_NODE = '<';
		private const char END_NODE = '>';
		private const char SLASH = '/';
		private const char EQUAL_SIGN = '=';
		private const char QUOTE_MARK = '"';

		private String text;

		private int curIndex;

		private bool error;
		public string errorString;

		public XMLParser() {
		} //

		public XMLNode Parse( string plainText ) {

			this.error = false;
			this.curIndex = 0;

			this.text = plainText;

			XMLNode node = this.ReadXMLNode();
			if ( node == null ) {
				return null;
			}

			return node;

		} //

		/**
		 * read an xml node that has an opening tag.
		 */
		private XMLNode ReadXMLNode() {

			bool closedTag = false;
			XMLNode node = new XMLNode();

			if ( !ReadStartTag( node, ref closedTag ) ) {

				// error
				return null;
			}

			if ( closedTag ) {
				// node was self-contained: <node />
				return node;
			}

			// if the node turns out to be a text node, need to return to this index
			// because any white space will be part of the text.
			int saveIndex = curIndex;

			Char next = this.GetNextNonWhite();
			XMLNode child;

			if ( next == START_NODE ) {

				do {

					next = this.GetNextNonWhite();

					// backtrack so the '<' and what follows can be read by further parsing.
					this.curIndex = saveIndex;

					if ( next == SLASH ) {

						// found the start of an end node: "</"
						// this has to be the end tag for this node, or there is an error.
						break;

					} //

					child = this.ReadXMLNode();
					if ( child == null ) {
						return null;
					}
					node.AddChild( child );

					// save position to return to the spot right before the '<' tag.
					saveIndex = this.curIndex;
					// get what should be the next '<' tag.
					next = this.GetNextNonWhite();

				} while ( next == START_NODE );

			} else {

				// assume the node is text. attempt to parse text.
				this.curIndex = saveIndex;
				if ( !this.ReadNodeText( node ) ) {
					Debug.Log( "error reading node text" );
					return null;
				}

			} //

			if ( !ReadEndTag( node ) ) {
				Debug.Log( "error reading end tag: " + node.NodeName);
				return null;
			}

			return node;

		} //

		/**
		 * returns false in case of error. closedTag returns true if the node is self-terminating: <node />
		 */
		private bool ReadStartTag( XMLNode node, ref bool closedTag ) {

			if ( !MatchChar( START_NODE ) ) {
				Debug.Log( "error reading start tag" );
				this.error = true;
				return false;
			}

			string nodeName = this.ReadIdentifier();
			if ( this.error || nodeName == String.Empty ) {
				Debug.Log( "error reading node name" );
				return false;
			}
			node.NodeName = nodeName;

			Char next = this.GetNextNonWhite();

			// READ ATTRIBUTES UNTIL THEY RUN OUT.
			while ( Char.IsLetter( next ) ) {

				// backtrack to the character.
				this.curIndex--;

				if ( !this.ReadAttribute( node ) ) {
					this.error = true;
					Debug.Log( "error reading attribute: " + node.NodeName );
					return false;
				}

				next = this.GetNextNonWhite();

			} //

			// READ END TAG.
			if ( next == SLASH ) {

				next = this.GetNextNonWhite();
				if ( next == END_NODE ) {

					closedTag = true;
					return true;

				}
				Debug.Log( "error reading final end />: " + node.NodeName );

			} else if ( next == END_NODE ) {

				closedTag = false;
				return true;

			}

			Debug.Log( "error ending node: " + node.NodeName );

			return false;

		} //

		/**
		 * like read start tag but much simpler...
		 */
		private bool ReadEndTag( XMLNode node ) {

			if ( !MatchChar( START_NODE ) ) {
				this.error = true;
				return false;
			}

			if ( !MatchChar( SLASH ) ) {
				this.error = true;
				return false;
			}

			// Read node name.
			string nodeName = this.ReadIdentifier();
			if ( this.error || nodeName == String.Empty || nodeName != node.NodeName ) {
				return false;
			}

			// close node
			return MatchChar( END_NODE );

		} //

		/**
		 * read a valid identifier, such as a node name, or attribute name.
		 **/
		private string ReadIdentifier() {

			char c = GetNextNonWhite();

			if ( !Char.IsLetter( c ) ) {
				this.error = true;
				return String.Empty;
			}

			int startIndex = this.curIndex - 1;

			if ( this.GetNextNonAlpha() == EOF_CHAR ) {
				this.error = true;
				return String.Empty;
			}

			// backtrack so the non-alpha char can be read by the next parser phase.
			// need to actually backtrack two since GetNextNonAlpha() advances past even the non-alphanumeric char.
			this.curIndex--;

			return this.text.Substring( startIndex, this.curIndex - startIndex );

		} //

		/**
		 * attempts to read a name="value" attribute, and returns false
		 * if parsing fails.
		 */
		private Boolean ReadAttribute( XMLNode node ) {

			string attr = this.ReadIdentifier();

			MatchChar( EQUAL_SIGN );
			MatchChar( QUOTE_MARK );

			string value = this.ReadText( QUOTE_MARK );

			MatchChar( QUOTE_MARK );

			// assign the attribute in the xml node.
			node[ attr ] = value;

			return true;

		} //

		/**
		 * read text until the terminal char is reached.
		 */
		private string ReadText( Char terminal ) {

			int textStart = this.curIndex;

			// TO-DO: add support for escape characters.
			// to-never-do: add support for <<[CDATA[ tags

			Char c;

			// keep going until the start of a new XML node is reached.
			while ( this.curIndex < this.text.Length ) {

				c = this.text[ this.curIndex++ ];

				if ( c == terminal ) {

					// backtrack from the terminal so it shows up on next read.
					this.curIndex--;

					return this.text.Substring( textStart, curIndex - textStart );

				} //

			} //

			// EOF reached.
			this.error = true;
			return String.Empty;

		} //

		private bool ReadNodeText( XMLNode node ) {

			int textStart = this.curIndex;

			// TO-DO: add support for escape characters.
			// to-never-do: add support for <<[CDATA[ tags

			Char c;

			// keep going until the start of a new XML node is reached.
			while ( this.curIndex < this.text.Length ) {

				c = this.text[ this.curIndex++ ];
				if ( c == START_NODE ) {

					// backtrack from '<' so it shows up on next read.
					this.curIndex--;

					node.NodeText = this.text.Substring( textStart, curIndex - textStart );
					return true;

				} //

			} //

			// EOF reached.
			this.error = true;
			return false;

		} //

		/**
		 * the next non-white char must be the match, or function returns false
		 */
		private Boolean MatchChar( Char match ) {

			char c;

			while ( this.curIndex < this.text.Length ) {

				c = this.text[ this.curIndex++ ];
				if ( c == match ) {
					return true;
				} else if ( !Char.IsWhiteSpace( c ) ) {
					// failed: found unexpected character.
					return false;
				}

			} //

			return false;

		} //

		/**
		 * find the next non-alphanumeric char
		 */
		private Char GetNextNonAlpha() {

			char c;

			while ( this.curIndex < this.text.Length ) {

				c = this.text[ this.curIndex++ ];
				if ( !Char.IsLetterOrDigit( c ) ) {
					return c;
				}

			} //

			return EOF_CHAR;

		} //

		private Char GetNextNonWhite() {

			char c;

			while ( this.curIndex < this.text.Length ) {

				c = this.text[ this.curIndex++ ];
				if ( !Char.IsWhiteSpace( c ) ) {
					return c;
				}

			} //

			return EOF_CHAR;

		} //

	} // class

} //
