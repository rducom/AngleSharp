﻿namespace AngleSharp.Dom.Html
{
    using AngleSharp.Dom.Collections;
    using AngleSharp.Dom.Css;
    using AngleSharp.Extensions;
    using AngleSharp.Html;
    using AngleSharp.Linq;
    using System;

    /// <summary>
    /// Represents the HTML tr element.
    /// </summary>
    sealed class HtmlTableRowElement : HtmlElement, IHtmlTableRowElement
    {
        #region Fields

        readonly HtmlCollection<HtmlTableCellElement> _cells;

        #endregion

        #region ctor

        public HtmlTableRowElement(Document owner)
            : base(owner, Tags.Tr, NodeFlags.Special | NodeFlags.ImplicitelyClosed)
        {
            _cells = new HtmlCollection<HtmlTableCellElement>(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value of the alignment attribute.
        /// </summary>
        public HorizontalAlignment Align
        {
            get { return GetAttribute(AttributeNames.Align).ToEnum(HorizontalAlignment.Left); }
            set { SetAttribute(AttributeNames.Align, value.ToString()); }
        }

        /// <summary>
        /// Gets or sets the value of the vertical alignment attribute.
        /// </summary>
        public VerticalAlignment VAlign
        {
            get { return GetAttribute(AttributeNames.Valign).ToEnum(VerticalAlignment.Middle); }
            set { SetAttribute(AttributeNames.Valign, value.ToString()); }
        }

        /// <summary>
        /// Gets or sets the value of the background color attribute.
        /// </summary>
        public String BgColor
        {
            get { return GetAttribute(AttributeNames.BgColor); }
            set { SetAttribute(AttributeNames.BgColor, value); }
        }

        /// <summary>
        /// Gets the assigned table cells.
        /// </summary>
        public IHtmlCollection Cells
        {
            get { return _cells; }
        }

        /// <summary>
        /// Gets the index in the logical order and not in document order. 
        /// </summary>
        public Int32 Index
        {
            get
            {
                var parent = ParentElement;

                while (parent != null && !(parent is HtmlTableElement))
                    parent = parent.ParentElement;

                var table = parent as IHtmlTableElement;

                if (table != null)
                    return table.Rows.Index(this);

                return 0;
            }
        }

        /// <summary>
        /// Gets the index of this row, relative to the current section starting from 0.
        /// </summary>
        public Int32 IndexInSection
        {
            get
            {
                var parent = ParentElement;

                while (parent != null && !(parent is IHtmlTableSectionElement))
                    parent = parent.ParentElement;

                var section = parent as IHtmlTableSectionElement;

                if (section != null)
                    return section.Rows.Index(this);

                return 0; 
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert an empty TD cell into this row. If index is -1 or equal to the number
        /// of cells, the new cell is appended.
        /// </summary>
        /// <param name="index">The place to insert the cell, starting from 0.</param>
        /// <returns>The inserted table cell.</returns>
        public IHtmlElement InsertCellAt(Int32 index = -1)
        {
            var cell = _cells[index];
            var newCell = Owner.CreateElement(Tags.Td) as HtmlTableCellElement;

            if (cell != null)
                InsertBefore(newCell, cell);
            else
                AppendChild(newCell);

            return newCell;
        }

        /// <summary>
        /// Deletes a cell from the current row.
        /// </summary>
        /// <param name="index">The index of the cell to delete, starting from 0. If the index is
        /// -1 the last cell in the row is deleted.</param>
        /// <returns>The current row.</returns>
        public void RemoveCellAt(Int32 index)
        {
            if (index == -1)
                index = _cells.Length - 1;

            var cell = _cells[index];

            if (cell != null)
                cell.Remove();
        }

        #endregion
    }
}
