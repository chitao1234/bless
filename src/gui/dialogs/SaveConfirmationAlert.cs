// created on 10/26/2004 at 7:01 PM
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
using System;
using Gtk;
using Mono.Unix;

namespace Bless.Gui.Dialogs {

///<summary>An alert dialog box as recommended in the Gnome HIG</summary>
public class SaveConfirmationAlert : Alert
{
	public SaveConfirmationAlert(string primary, Gtk.Window parent)
			: base(string.Format(Catalog.GetString("Save changes to file '{0}' before closing?"), primary),
				   Catalog.GetString("If you don't save, all changes made since the last save will be lost."), parent)
	{
		image.SetFromIconName("dialog-warning", Gtk.IconSize.Dialog);

		this.AddButton(Catalog.GetString("Close without Saving"), ResponseType.No);
		Gtk.Button cancelButton = (Gtk.Button)this.AddButton(Catalog.GetString("Cancel"), ResponseType.Cancel);
		cancelButton.Image = Gtk.Image.NewFromIconName("dialog-cancel", Gtk.IconSize.Button);
		cancelButton.AlwaysShowImage = true;
		Gtk.Button saveButton = (Gtk.Button)this.AddButton(Catalog.GetString("Save"), ResponseType.Ok);
		saveButton.Image = Gtk.Image.NewFromIconName("document-save", Gtk.IconSize.Button);
		saveButton.AlwaysShowImage = true;

		this.DefaultResponse = ResponseType.Cancel;

		this.ShowAll();
	}

}



}