// created on 6/14/2004 at 10:39 PM
/*
 *   Copyright (c) 2004, Alexandros Frantzis (alf82 [at] freemail [dot] gr)
 *
 *   This file is part of Bless.
 *
 *   Bless is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *   Bless is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Bless; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using Bless.Gui.Drawers;
using Bless.Util;
using Bless.Buffers;
using Bless.Tools.Find;
using Cairo;
using Gdk;
using System.Collections.Generic;
using System.Xml;

namespace Bless.Gui.Areas {

/// <summary>
/// An area on the screen that displays data in a specific way.
/// </summary>
public abstract class Area
{
	protected AreaGroup areaGroup;
	protected Drawer drawer;
	protected Drawer.Information drawerInformation;
	protected string type;

	// display
	protected int x;
	protected int y;
        protected int width;
        protected int height;
        protected int bpr;
        protected int dpb; // digits per byte
        protected int fixedBpr;
        protected bool isAreaRealized;

        protected Gdk.Color cursorColor;
        protected Gdk.Color activeCursorColor;
        protected Gdk.Color inactiveCursorColor;

        protected Cairo.Context renderContext;

	
	protected int cursorDigit;
        protected bool cursorFocus;
        protected bool isActive;
        protected bool canFocus;
	
	public enum RenderMergeFlags {None = 0, Left = 1, Right = 2}
	// Abstract methods
	
	/// <summary>
	/// Render (a part of) a row normally (without highlighting)
	/// </summary>
	/// <param name="i">
	/// The row to render.
	/// </param>
	/// <param name="p">
	/// The offset of the byte in the row to start rendering.
	/// </param>
	/// <param name="n">
	/// The number of bytes to render.
	/// </param>
	/// <param name="blank">
	/// Whether to blank the background before rendering.
	/// </param>
	abstract protected void RenderRowNormal(int i, int p, int n, bool blank);
	
	/// <summary>
	/// Render (a part of) a row with highlighting.
	/// </summary>
	/// <param name="i">
	/// The row to render.
	/// </param>
	/// <param name="p">
	/// The offset of the byte in the row to start rendering.
	/// </param>
	/// <param name="n">
	/// The number of bytes to render.
	/// </param>
	/// <param name="blank">
	/// Whether to blank the background before rendering.
	/// </param>
	abstract protected void RenderRowHighlight(int i, int p, int n, bool blank, Drawer.HighlightType ht);

	/// <summary>
	/// Get info about where a specified buffer offset is currently displayed.
	/// </summary>
	/// <param name="off">
	/// The offset to query.
	/// </param>
	/// <param name="orow">
	/// The row the offset is displayed in.
	/// </param>
	/// <param name="obyte">
	/// The byte in the row the offset is displayed on.
	/// </param>
	/// <param name="ox">
	/// The X coordinate of the displayed position.
	/// </param>
	/// <param name="oy">
	/// The Y coordinate of the displayed position.
	/// </param>
	abstract public void GetDisplayInfoByOffset(long off, out int orow, out int obyte, out int ox, out int oy);

	/// <summary>
	/// Flags containing extra info about positions in the view. 
	/// </summary>
	public enum GetOffsetFlags {
		/// <summary>Position is beyond the end of file</summary>
		Eof = 1,
		/// <summary>Position is between bytes (in the so-called abyss)</summary>
		Abyss = 2
	}
	
	/// <summary>
	/// Gets the offset in the buffer that is displayed at specified location in the area
	/// </summary>
	/// <param name="x">
	/// The X coordinate of the queried location.
	/// </param>
	/// <param name="y">
	/// The Y coordinate of the queried location.
	/// </param>
	/// <param name="digit">
	/// The digit of the byte at the specified location.
	/// </param>
	/// <param name="rflags">
	/// Some flags with additional info about the location (<see cref="GetOffsetFlags"/>).
	/// </param>
	/// <returns>
	/// The offset of the byte at the specified location.
	/// </returns>
	abstract public long GetOffsetByDisplayInfo(int x, int y, out int digit, out GetOffsetFlags rflags);

	/// <summary>
	/// Special key handler for the area. 
	/// </summary>
	/// <param name="key">
	/// The key that was pressed.
	/// </param>
	/// <param name="overwrite">
	/// Whether we are in overwrite mode.
	/// </param>
	/// <returns>
	/// Whether the key was actually handled.
	/// </returns>
	virtual public bool HandleKey(Gdk.Key key, bool overwrite)
	{
		return false;
	}

	///<summary>
	/// Calculates the width in pixels the
	/// area will occupy with n bytes per row.
	///</summary>
	abstract public int CalcWidth(int n, bool force);

	/// <summary>
	/// A delegate that creates instances of area types.
	/// </summary>
	public delegate Area AreaCreatorFunc(AreaGroup ag);
	
	/// <summary>
	/// A dictionary to hold area type names and creation functions
	/// </summary>
	static private Dictionary<string, AreaCreatorFunc> pluginTable;
	
	/// <summary>
	/// Adds a new area type to the factory.
	/// </summary>
	/// <param name="name">
	/// The name of the area type
	/// </param>
	/// <param name="createArea">
	/// The delegate to call to create an instance of this area type.
	/// </param>
	/// <remarks>This is used by plugins to register new area types.</remarks>
	static public void AddFactoryItem(string name, AreaCreatorFunc createArea)
	{
		if (pluginTable == null) {
			pluginTable = new Dictionary<string, AreaCreatorFunc>();
		}
		// System.Console.WriteLine("Adding plugin name {0}", name);
		pluginTable.Add(name, createArea);
	}

	/// <summary>
	/// Factory method to create instances of area types by name.
	/// </summary>
	/// <param name="name">
	/// The name of the type of the area to create.
	/// </param>
	/// <param name="ag">
	/// The <see cref="AreaGroup"/> this area will belong in.
	/// </param>
	/// <returns>
	/// The created area.
	/// </returns>
	static public Area Factory(string name, AreaGroup ag)
	{
		try {
			AreaCreatorFunc acf = pluginTable[name];
			return acf(ag);
		}
		catch (KeyNotFoundException e) {
			System.Console.WriteLine(e.Message);
		}

		return null;
	}

	
	/// <summary>
	/// Create an area.
	/// </summary>
	/// <param name="areaGroup">
	/// The <see cref="AreaGroup"/> this area will belong in.
	/// </param>
	public Area(AreaGroup areaGroup)
	{
		this.areaGroup = areaGroup;
		
		drawerInformation = new Drawer.Information();

		canFocus = false;
		dpb = 0;
		fixedBpr = -1;
		isAreaRealized = false;
	}

	/// <summary>
	/// Configure an area using an xml configuration.
	/// </summary>
	/// <param name="parentNode">
	/// The <see cref="XmlNode"/> that holds the configuration for this area.
	/// </param>
	/// <remarks>This is overriden by inherited classes to accept extra options</remarks>
	public virtual void Configure(XmlNode parentNode)
	{
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "bpr")
				this.FixedBytesPerRow = System.Convert.ToInt32(node.InnerText);
			if (node.Name == "display")
				ParseDisplay(node, drawerInformation);
		}
	}

	///<summary>Parse the <display> tag in layout files</summary>
	void ParseDisplay(XmlNode parentNode, Drawer.Information info)
	{
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			if (node.Name == "evenrow")
				ParseDisplayRow(node, info, Drawer.RowType.Even);
			else if (node.Name == "oddrow")
				ParseDisplayRow(node, info, Drawer.RowType.Odd);
			else if (node.Name == "font")
				info.FontName = node.InnerText;
		}
	}

	void ParseDisplayRow(XmlNode parentNode, Drawer.Information info, Drawer.RowType rowType)
	{
		Drawer.Color fg, bg;
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			ParseDisplayType(node, out fg, out bg);
			if (node.Name == "evencolumn") {
				if (bg != null)
					info.bgNormal[(int)rowType, (int)Drawer.ColumnType.Even] = bg;
				if (fg != null)
					info.fgNormal[(int)rowType, (int)Drawer.ColumnType.Even] = fg;
			}
			else if (node.Name == "oddcolumn") {
				if (bg != null)
					info.bgNormal[(int)rowType, (int)Drawer.ColumnType.Odd] = bg;
				if (fg != null)
					info.fgNormal[(int)rowType, (int)Drawer.ColumnType.Odd] = fg;
			}
			else if (node.Name == "selectedcolumn") {
				if (bg != null)
					info.bgHighlight[(int)rowType, (int)Drawer.HighlightType.Selection] = bg;
				if (fg != null)
					info.fgHighlight[(int)rowType, (int)Drawer.HighlightType.Selection] = fg;
			}
			else if (node.Name == "patternmatchcolumn") {
				if (bg != null)
					info.bgHighlight[(int)rowType, (int)Drawer.HighlightType.PatternMatch] = bg;
				if (fg != null)
					info.fgHighlight[(int)rowType, (int)Drawer.HighlightType.PatternMatch] = fg;
			}
		}
	}

	///<summary>Parse a font type</summary>
	void ParseDisplayType(XmlNode parentNode, out Drawer.Color fg, out Drawer.Color bg)
	{
		fg = null;
		bg = null;
		XmlNodeList childNodes = parentNode.ChildNodes;
		foreach(XmlNode node in childNodes) {
			Gdk.Color col = new Gdk.Color();
                        if (node.Name == "foreground") {
                                Gdk.Color.Parse(node.InnerText, ref col);
                                fg = new Drawer.Color(col);
                        }
                        if (node.Name == "background") {
                                Gdk.Color.Parse(node.InnerText, ref col);
                                bg = new Drawer.Color(col);
                        }
		}
	}

	///<summary>
	/// Realize the area.
	///</summary>
        public virtual void Realize()
        {
                Gtk.DrawingArea da = areaGroup.DrawingArea;

                Gdk.Color col = new Gdk.Color();

                Gdk.Color.Parse("red", ref col);
                activeCursorColor = col;
                Gdk.Color.Parse("gray", ref col);
                inactiveCursorColor = col;
                cursorColor = activeCursorColor;

                isAreaRealized = true;
        }

        internal void SetRenderContext(Cairo.Context context)
        {
                renderContext = context;
        }

        internal void ClearRenderContext()
        {
                renderContext = null;
        }

        protected void FillRectangle(Drawer.Color color, int rx, int ry, int w, int h)
        {
                if (renderContext == null || color == null)
                        return;

                renderContext.Save();
                Gdk.CairoHelper.SetSourceColor(renderContext, color.GdkColor);
                renderContext.Rectangle(rx, ry, w, h);
                renderContext.Fill();
                renderContext.Restore();
        }

        protected void FillRectangle(Gdk.Color color, int rx, int ry, int w, int h)
        {
                if (renderContext == null)
                        return;

                renderContext.Save();
                Gdk.CairoHelper.SetSourceColor(renderContext, color);
                renderContext.Rectangle(rx, ry, w, h);
                renderContext.Fill();
                renderContext.Restore();
        }

        protected void DrawLine(Gdk.Color color, int x1, int y1, int x2, int y2)
        {
                if (renderContext == null)
                        return;

                renderContext.Save();
                Gdk.CairoHelper.SetSourceColor(renderContext, color);
                renderContext.LineWidth = 1.0;
                renderContext.MoveTo(x1 + 0.5, y1 + 0.5);
                renderContext.LineTo(x2 + 0.5, y2 + 0.5);
                renderContext.Stroke();
                renderContext.Restore();
        }

	// Properties
	//
	public int X {
		set { x = value; }
		get	{ return x; }
	}

	public int Y {
		set { y = value; }
		get	{ return y; }
	}

	public int Width {
		set { width = value; }
		get	{ return width; }
	}

	public int Height {
		set { height = value; }
		get	{ return height; }
	}

	public int BytesPerRow {
		set { bpr = value; }
		get	{ return bpr; }
	}

	public int FixedBytesPerRow {
		set { fixedBpr = value; }
		get { return fixedBpr; }
	}

	public int DigitsPerByte {
		get	{ return dpb; }
	}

	public int CursorDigit {
		get { return cursorDigit; }
		set { if (value >= dpb) cursorDigit = dpb - 1; else cursorDigit = value; }
	}
	
        public bool HasCursorFocus {
                set {
                        cursorFocus = value;
                        cursorColor = cursorFocus ? activeCursorColor : inactiveCursorColor;
                }
                get { return cursorFocus;}
        }

	public bool CanFocus {
		get { return canFocus;}
	}

	public Drawer.Information DrawerInformation
	{
		get { return drawerInformation; }
		set { drawerInformation = value; }
	}

	public virtual Drawer Drawer {
		get { return drawer; }
	}

	public string Type {
		get { return type; }
	}

        public bool IsActive {
                set {
                        isActive = value;
                        cursorColor = isActive ? activeCursorColor : inactiveCursorColor;
                        // doesn't actually change cursor position
                        // just redraws it with correct color
                        //MoveCursor(cursorOffset, cursorDigit);
                }

                get { return isActive; }
        }

	/// <summary>
	/// This method is only called to draw extra stuff when doing a complete
	/// redraw of the area
	/// </summary>
	internal protected virtual void RenderExtra()
	{
	
	}
	
	
	void RenderRangeHelper(Drawer.HighlightType ht, int rstart, int bstart, int len)
	{
		if (ht != Drawer.HighlightType.Normal)
			RenderRowHighlight(rstart, bstart, len, false, ht);
		else
			RenderRowNormal(rstart, bstart, len, false);
	}


	/// <summary>
	/// Render the bytes in the highlight range.
	/// </summary>
	/// <param name="h">
	/// A <see cref="Highlight"/>
	/// </param>
	/// <param name="merge">
	/// Whether to visually merge the highlight with adjacent ones left and/or right.
	/// </param>
	/// <remarks>
	/// This method doesn't check whether the merge flags are correct (eg there is indeed
	/// a similar adjacent highlight). It just draws the highlight in such a way as 
	/// to appear merged to any similar highlights if they exist.
	///</remarks>
        internal protected virtual void RenderHighlight(Highlight h, Drawer.HighlightType left, Drawer.HighlightType right)
        {
                if (isAreaRealized == false)
                        return;

                int rstart, bstart, xstart, ystart;
                int rend, bend, xend, yend;

                Drawer.Color oddColor = drawer.GetBackgroundColor(Drawer.RowType.Odd, h.Type);
                Drawer.Color evenColor = drawer.GetBackgroundColor(Drawer.RowType.Even, h.Type);

                GetDisplayInfoByOffset(h.Start, out rstart, out bstart, out xstart, out ystart);
                GetDisplayInfoByOffset(h.End, out rend, out bend, out xend, out yend);

                bool drawLeft = false;
                int dxstart = xstart;

                if (bstart > 0) {
                        int digit;
                        GetOffsetFlags gof;
                        GetOffsetByDisplayInfo(xstart - 1, ystart, out digit, out gof);
                        if ((gof & GetOffsetFlags.Abyss) != 0) {
                                dxstart -= drawer.Width;
                                drawLeft = true;
                        }
                }

                bool drawRight = false;
                int dxend = xend;

                if (bend < bpr - 1) {
                        int digit;
                        GetOffsetFlags gof;
                        GetOffsetByDisplayInfo(xend + dpb*drawer.Width, yend, out digit, out gof);
                        if ((gof & GetOffsetFlags.Abyss) != 0) {
                                dxend += drawer.Width;
                                drawRight = true;
                        }
                }

                if (rstart == rend) {
                        bool odd = (((h.Start / bpr) % 2) == 1);
                        Drawer.Color baseColor = odd ? oddColor : evenColor;
                        Drawer.Color leftColor = drawer.GetBackgroundColor(odd ? Drawer.RowType.Odd : Drawer.RowType.Even, left);
                        Drawer.Color rightColor = drawer.GetBackgroundColor(odd ? Drawer.RowType.Odd : Drawer.RowType.Even, right);

                        if (drawLeft)
                                FillRectangle(leftColor, x + dxstart, y + ystart, drawer.Width, drawer.Height);
                        if (drawRight)
                                FillRectangle(rightColor, x + xend + dpb*drawer.Width, y + yend, drawer.Width, drawer.Height);

                        FillRectangle(baseColor, x + xstart, y + ystart, xend - xstart + dpb*drawer.Width, drawer.Height);

                        RenderRangeHelper(h.Type, rstart, bstart, bend - bstart + 1);
                }
                else {
                        bool odd = (((h.Start / bpr) % 2) == 1);
                        Drawer.Color baseColor = odd ? oddColor : evenColor;
                        Drawer.Color leftColor = drawer.GetBackgroundColor(odd ? Drawer.RowType.Odd : Drawer.RowType.Even, left);

                        if (drawLeft)
                                FillRectangle(leftColor, x + dxstart, y + ystart, drawer.Width, drawer.Height);
                        FillRectangle(baseColor, x + xstart, y + ystart, width - xstart, drawer.Height);

                        RenderRangeHelper(h.Type, rstart, bstart, bpr - bstart);

                        long curOffset = h.Start + bpr - bstart;

                        for (int i = rstart + 1; i < rend; i++) {
                                odd = (((curOffset / bpr) % 2) == 1);
                                baseColor = odd ? oddColor : evenColor;
                                FillRectangle(baseColor, x, y + i*drawer.Height, width, drawer.Height);
                                RenderRangeHelper(h.Type, i, 0, bpr);
                                curOffset += bpr;
                        }

                        odd = (((h.End / bpr) % 2) == 1);
                        baseColor = odd ? oddColor : evenColor;
                        Drawer.Color rightColor = drawer.GetBackgroundColor(odd ? Drawer.RowType.Odd : Drawer.RowType.Even, right);

                        if (drawRight)
                                FillRectangle(rightColor, x + xend + dpb*drawer.Width, y + yend, drawer.Width, drawer.Height);
                        FillRectangle(baseColor, x, y + yend, xend + dpb*drawer.Width, drawer.Height);
                        RenderRangeHelper(h.Type, rend, 0, bend + 1);
                }
        }
	
	/// <summary>
	/// Blanks the selected offset
	/// </summary>
	/// <param name="offs">
	/// A <see cref="System.Int64"/>
	/// </param>
        internal protected void BlankOffset(long offs)
        {
                if (isAreaRealized == false)
                        return;

                int nrows = height / drawer.Height;
                long bytesInView = nrows * bpr;

                if (offs >= areaGroup.Offset && offs < areaGroup.Offset + bytesInView) {
                        int pcRow, pcByte, pcX, pcY;
                        GetDisplayInfoByOffset(offs, out pcRow, out pcByte, out pcX, out pcY);
                        Drawer.Color backEven = drawer.GetBackgroundColor(Drawer.RowType.Even, Drawer.HighlightType.Normal);
                        FillRectangle(backEven, x + pcX, y + pcY, drawer.Width*dpb, drawer.Height);
                }

        }

        ///<summary>Render the cursor</summary>
        internal protected void RenderCursor()
        {
                if (isAreaRealized == false)
                        return;

                int cRow, cByte, cX, cY;
                GetDisplayInfoByOffset(areaGroup.CursorOffset, out cRow, out cByte, out cX, out cY);

                FillRectangle(cursorColor, x + cX, y + cY + drawer.Height - 2, drawer.Width*dpb, 2);
                if (cursorFocus) {
                        FillRectangle(cursorColor, x + cX + cursorDigit*drawer.Width, y + cY, 1, drawer.Height - 2);
                }
        }
	
	/// <summary>
	/// Dispose the (server side) pixmaps used by this area.
	/// </summary>
        public void DisposePixmaps()
        {
                if (isAreaRealized == false)
                        return;

                drawer.DisposePixmaps();
        }

	
        internal virtual void BlankBackground()
        {
                Drawer.Color backEven = drawer.GetBackgroundColor(Drawer.RowType.Even, Drawer.HighlightType.Normal);
                FillRectangle(backEven, x, y, width, height);
        }

        internal virtual void BlankEof()
        {
                int pcRow, pcByte, pcX, pcY;
                GetDisplayInfoByOffset(areaGroup.Buffer.Size, out pcRow, out pcByte, out pcX, out pcY);
                Drawer.Color backEven = drawer.GetBackgroundColor(Drawer.RowType.Even, Drawer.HighlightType.Normal);
                FillRectangle(backEven, x + pcX, y + pcY, drawer.Width*dpb, drawer.Height);
        }
	
	public virtual void ShowPopup(Gtk.UIManager uim)
	{
		Gtk.Widget popup = uim.GetWidget("/DefaultAreaPopup");
		(popup as Gtk.Menu).Popup();
	}

}// Area


} //namespace
