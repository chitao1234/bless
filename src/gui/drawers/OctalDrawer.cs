// created on 7/1/2004 at 8:41 PM
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

///<summary>Draws the octal representation of a byte</summary>
public class OctalDrawer : Drawer {

	static readonly string OctalTable = "000001002003004005006007010011012013014015016017020021022023024025026027030031032033034035036037040041042043044045046047050051052053054055056057060061062063064065066067070071072073074075076077100101102103104105106107110111112113114115116117120121122123124125126127130131132133134135136137140141142143144145146147150151152153154155156157160161162163164165166167170171172173174175176177200201202203204205206207210211212213214215216217220221222223224225226227230231232233234235236237240241242243244245246247250251252253254255256257260261262263264265266267270271272273274275276277300301302303304305306307310311312313314315316317320321322323324325326327330331332333334335336337340341342343344345346347350351352353354355356357360361362363364365366367370371372373374375376377";

	public OctalDrawer(Gtk.Widget wid, Information inf)
			: base(wid, inf)
	{
	}

        protected override void Draw(Cairo.Context cr, int x, int y, byte b, ImageSurface surface)
        {
                if (surface == null)
                        return;

                cr.Save();
                cr.Rectangle(x, y, 3*width, height);
                cr.SetSourceSurface(surface, x - b*3*width, y);
                cr.Fill();
                cr.Restore();
        }

        protected override ImageSurface Create(Gdk.Color fg, Gdk.Color bg)
        {
                int surfaceWidth = 256*3*width;
                ImageSurface surface = new ImageSurface(Format.Argb32, surfaceWidth, height);

                using (Cairo.Context cr = new Cairo.Context(surface)) {
                        Gdk.CairoHelper.SetSourceColor(cr, bg);
                        cr.Rectangle(0, 0, surfaceWidth, height);
                        cr.Fill();

                        string s = OctalDrawer.OctalTable;

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