// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision$</version>
// </file>

using System;

namespace ICSharpCode.TextEditor.Document
{
    public enum AnchorMovementType
    {
        /// <summary>
        /// Behaves like a start marker - when text is inserted at the anchor position, the anchor will stay
        /// before the inserted text.
        /// </summary>
        BeforeInsertion,
        /// <summary>
        /// Behave like an end marker - when text is insered at the anchor position, the anchor will move
        /// after the inserted text.
        /// </summary>
        AfterInsertion
    }

    /// <summary>
    /// An anchor that can be put into a document and moves around when the document is changed.
    /// </summary>
    public sealed class TextAnchor
    {
        private static Exception AnchorDeletedError()
        {
            return new InvalidOperationException("The text containing the anchor was deleted");
        }

        private LineSegment lineSegment;
        private int columnNumber;

        public LineSegment Line {
            get {
                if (lineSegment == null) throw AnchorDeletedError();
                return lineSegment;
            }
            internal set => lineSegment = value;
        }

        public bool IsDeleted => lineSegment == null;

        public int LineNumber => Line.LineNumber;

        public int ColumnNumber {
            get {
                if (lineSegment == null) throw AnchorDeletedError();
                return columnNumber;
            }
            internal set => columnNumber = value;
        }

        public TextLocation Location => new TextLocation(ColumnNumber, LineNumber);

        public int Offset => Line.Offset + columnNumber;

        /// <summary>
        /// Controls how the anchor moves.
        /// </summary>
        public AnchorMovementType MovementType { get; set; }

        public event EventHandler Deleted;

        internal void Delete(ref DeferredEventList deferredEventList)
        {
            // we cannot fire an event here because this method is called while the LineManager adjusts the
            // lineCollection, so an event handler could see inconsistent state
            lineSegment = null;
            deferredEventList.AddDeletedAnchor(this);
        }

        internal void RaiseDeleted()
        {
            if (Deleted != null)
                Deleted(this, EventArgs.Empty);
        }

        internal TextAnchor(LineSegment lineSegment, int columnNumber)
        {
            this.lineSegment = lineSegment;
            this.columnNumber = columnNumber;
        }

        public override string ToString()
        {
            if (IsDeleted)
                return "[TextAnchor (deleted)]";
            else
                return "[TextAnchor " + Location.ToString() + "]";
        }
    }
}
