﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Drawing;
using SWF = System.Windows.Forms;

namespace ICSharpCode.TextEditor.Document
{
    /// <summary>
    /// Description of Bookmark.
    /// </summary>
    public class Bookmark
    {
        private IDocument document;
        private TextLocation location;
        private bool isEnabled = true;

        public IDocument Document {
            get => document;
            set {
                if (document != value) {
                    if (Anchor != null) {
                        location = Anchor.Location;
                        Anchor = null;
                    }
                    document = value;
                    CreateAnchor();
                    OnDocumentChanged(EventArgs.Empty);
                }
            }
        }

        private void CreateAnchor()
        {
            if (document != null) {
                LineSegment line = document.GetLineSegment(Math.Max(0, Math.Min(location.Line, document.TotalNumberOfLines-1)));
                Anchor = line.CreateAnchor(Math.Max(0, Math.Min(location.Column, line.Length)));
                // after insertion: keep bookmarks after the initial whitespace (see DefaultFormattingStrategy.SmartReplaceLine)
                Anchor.MovementType = AnchorMovementType.AfterInsertion;
                Anchor.Deleted += AnchorDeleted;
            }
        }

        private void AnchorDeleted(object sender, EventArgs e)
        {
            document.BookmarkManager.RemoveMark(this);
        }

        /// <summary>
        /// Gets the TextAnchor used for this bookmark.
        /// Is null if the bookmark is not connected to a document.
        /// </summary>
        public TextAnchor Anchor { get; private set; }

        public TextLocation Location {
            get {
                if (Anchor != null)
                    return Anchor.Location;
                else
                    return location;
            }
            set {
                location = value;
                CreateAnchor();
            }
        }

        public event EventHandler DocumentChanged;

        protected virtual void OnDocumentChanged(EventArgs e)
        {
            if (DocumentChanged != null) {
                DocumentChanged(this, e);
            }
        }

        public bool IsEnabled {
            get => isEnabled;
            set {
                if (isEnabled != value) {
                    isEnabled = value;
                    if (document != null) {
                        document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.SingleLine, LineNumber));
                        document.CommitUpdate();
                    }
                    OnIsEnabledChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler IsEnabledChanged;

        protected virtual void OnIsEnabledChanged(EventArgs e)
        {
            if (IsEnabledChanged != null) {
                IsEnabledChanged(this, e);
            }
        }

        public int LineNumber {
            get {
                if (Anchor != null)
                    return Anchor.LineNumber;
                else
                    return location.Line;
            }
        }

        public int ColumnNumber {
            get {
                if (Anchor != null)
                    return Anchor.ColumnNumber;
                else
                    return location.Column;
            }
        }

        /// <summary>
        /// Gets if the bookmark can be toggled off using the 'set/unset bookmark' command.
        /// </summary>
        public virtual bool CanToggle => true;

        public Bookmark(IDocument document, TextLocation location) : this(document, location, true)
        {
        }

        public Bookmark(IDocument document, TextLocation location, bool isEnabled)
        {
            this.document = document;
            this.isEnabled = isEnabled;
            Location = location;
        }

        public virtual bool Click(SWF.Control parent, SWF.MouseEventArgs e)
        {
            if (e.Button == SWF.MouseButtons.Left && CanToggle) {
                document.BookmarkManager.RemoveMark(this);
                return true;
            }
            return false;
        }

        public virtual void Draw(IconBarMargin margin, Graphics g, Point p)
        {
            margin.DrawBookmark(g, p.Y, isEnabled);
        }
    }
}
