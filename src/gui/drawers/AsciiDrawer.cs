// created on 6/28/2004 at 4:46 PM
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

using Cairo;
using Gdk;
using Pango;

namespace Bless.Gui.Drawers {

///<summary>Draws the ascii representation of a byte</summary>
public class AsciiDrawer : Drawer {

	// Use the Zero Width Non-Joiner character \u200c to avoid ligatures
	static readonly string AsciiTable = "................................ !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHI\u200cJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghi\u200cjklmnopqrs\u200ctuvwxyz{|}~.................................................................................................................................";

	public AsciiDrawer(Gtk.Widget wid, Information inf)
			: base(wid, inf)
	{
	}

        protected override void Draw(Cairo.Context cr, int x, int y, byte b, ImageSurface surface)
        {
                if (surface == null)
                        return;

                cr.Save();
                cr.Rectangle(x, y, width, height);
                cr.SetSourceSurface(surface, x - b*width, y);
                cr.Fill();
                cr.Restore();
        }

        protected override ImageSurface Create(Gdk.Color fg, Gdk.Color bg)
        {
                int surfaceWidth = 256*width;
                ImageSurface surface = new ImageSurface(Format.Argb32, surfaceWidth, height);

                using (Cairo.Context cr = new Cairo.Context(surface)) {
                        Gdk.CairoHelper.SetSourceColor(cr, bg);
                        cr.Rectangle(0, 0, surfaceWidth, height);
                        cr.Fill();

                        string s = AsciiDrawer.AsciiTable;

                        pangoLayout.SetText(s);
                        Gdk.CairoHelper.SetSourceColor(cr, fg);
                        Pango.CairoHelper.UpdateLayout(cr, pangoLayout);
                        cr.MoveTo(0, 0);
                        Pango.CairoHelper.ShowLayout(cr, pangoLayout);
                }

                surface.Flush();
                return surface;
        }

}

} //namespace
