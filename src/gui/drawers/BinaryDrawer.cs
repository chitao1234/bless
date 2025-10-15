// created on 6/28/2004 at 4:48 PM
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

///<summary>Draws the binary representation of a byte</summary>
public class BinaryDrawer : Drawer {

	public BinaryDrawer(Gtk.Widget wid, Information inf)
			: base(wid, inf)
	{
	}


        protected override void Draw(Cairo.Context cr, int x, int y, byte b, ImageSurface surface)
        {
                if (surface == null)
                        return;

                // draw from the end backwards
                x += 6 * width;
                for (int i = 0; i < 4; i++) {
                        byte k = (byte)(b & 3);
                        cr.Save();
                        cr.Rectangle(x, y, 2*width, height);
                        cr.SetSourceSurface(surface, x - k*2*width, y);
                        cr.Fill();
                        cr.Restore();
                        x -= 2 * width;
                        b = (byte)(b >> 2);
                }
        }

        protected override ImageSurface Create(Gdk.Color fg, Gdk.Color bg)
        {
                int surfaceWidth = 4*2*width;
                ImageSurface surface = new ImageSurface(Format.Argb32, surfaceWidth, height);

                using (Cairo.Context cr = new Cairo.Context(surface)) {
                        Gdk.CairoHelper.SetSourceColor(cr, bg);
                        cr.Rectangle(0, 0, surfaceWidth, height);
                        cr.Fill();

                        string s = "00011011";

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