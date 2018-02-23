/*

(c) 2004, Marc Clifton
All Rights Reserved

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, 
this list of conditions and the following disclaimer. 

Redistributions in binary form must reproduce the above copyright notice, 
this list of conditions and the following disclaimer in the documentation 
and/or other materials provided with the distribution. 

Neither the name of Marc Clifton nor the names of its contributors may 
be used to endorse or promote products derived from this software without 
specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;

namespace ImageViewer
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		protected int lastX=0;
		protected int lastY=0;
		protected string lastFilename=String.Empty;
		protected PictureBox thumbnail;
		protected DragDropEffects effect;
		protected bool validData;
		protected Image image;
		protected Image nextImage;
		protected Thread getImageThread;

		private System.Windows.Forms.PictureBox pb;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pb = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// pb
			// 
			//			this.pb.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pb.Location = new System.Drawing.Point(0, 0);
			this.pb.Name = "pb";
			this.pb.Size = new System.Drawing.Size(292, 266);
			this.pb.TabIndex = 0;
			this.pb.TabStop = false;
			this.pb.SizeMode=PictureBoxSizeMode.StretchImage;
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
			this.DragLeave += new System.EventHandler(this.OnDragLeave);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.OnDragOver);
			this.AllowDrop=true;

			thumbnail=new PictureBox();
			thumbnail.SizeMode=PictureBoxSizeMode.StretchImage;
			pb.Controls.Add(thumbnail);
			thumbnail.Visible=false;
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.pb);
			this.Name = "Form1";
			this.Text = "Image Viewer";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void OnDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			Debug.WriteLine("OnDragDrop");
			if (validData)
			{
				while (getImageThread.IsAlive)
				{
					Application.DoEvents();
					Thread.Sleep(0);
				}
				thumbnail.Visible=false;
				image=nextImage;
				AdjustView();
				if ( (pb.Image != null) && (pb.Image != nextImage) )
				{
					pb.Image.Dispose();
				}
				pb.Image=image;
			}
		}

		private void OnDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			Debug.WriteLine("OnDragEnter");
			string filename;
			validData=GetFilename(out filename, e);
			if (validData)
			{
				if (lastFilename != filename)
				{
					thumbnail.Image=null;
					thumbnail.Visible=false;
					lastFilename=filename;
					getImageThread=new Thread(new ThreadStart(LoadImage));
					getImageThread.Start();
				}
				else
				{
					thumbnail.Visible=true;
				}
				e.Effect=DragDropEffects.Copy;
			}
			else
			{
				e.Effect=DragDropEffects.None;
			}
		}

		private void OnDragLeave(object sender, System.EventArgs e)
		{
			Debug.WriteLine("OnDragLeave");
			thumbnail.Visible=false;
		}

		private void OnDragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			Debug.WriteLine("OnDragOver");
			if (validData)
			{
				if ( (e.X != lastX) || (e.Y != lastY) )
				{
					SetThumbnailLocation(this.PointToClient(new Point(e.X, e.Y)));
				}
			}
		}
																			   
		protected bool GetFilename(out string filename, DragEventArgs e)
		{
			bool ret=false;
			filename=String.Empty;

			if ( (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
			{
				Array data=((IDataObject)e.Data).GetData("FileDrop") as Array;
				if (data != null)
				{
					if ( (data.Length == 1) && (data.GetValue(0) is String) )
					{
						filename=((string[])data)[0];
						string ext=Path.GetExtension(filename).ToLower();
						if ( (ext==".jpg") || (ext==".png") || (ext==".bmp") )
						{
							ret=true;
						}
					}
				}
			}
			return ret;
		}

		protected void SetThumbnailLocation(Point p)
		{
			if (thumbnail.Image==null)
			{
				thumbnail.Visible=false;
			}
			else
			{
				p.X-=thumbnail.Width/2;
				p.Y-=thumbnail.Height/2;
				thumbnail.Location=p;
				thumbnail.Visible=true;
			}
		}

		protected void AdjustView()
		{	
			float fw=this.ClientSize.Width;
			float fh=this.ClientSize.Height;
			float iw=image.Width;
			float ih=image.Height;

			// iw/fw > ih/fh, then iw/fw controls ih

			float rw=fw/iw;			// ratio of width
			float rh=fh/ih;			// ratio of height

			if (rw < rh)
			{
				pb.Width=(int)fw;
				pb.Height=(int)(ih * rw);
				pb.Left=0;
				pb.Top=(int)((fh - pb.Height)/2);
			}
			else
			{
				pb.Width=(int)(iw * rh);
				pb.Height=(int)fh;
				pb.Left=(int)((fw - pb.Width)/2);
				pb.Top=0;
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (image != null)
			{
				AdjustView();
			}
		}

		public delegate void AssignImageDlgt();

		protected void LoadImage()
		{
			nextImage=new Bitmap(lastFilename);
			this.Invoke(new AssignImageDlgt(AssignImage));
		}

		protected void AssignImage()
		{
			thumbnail.Width=100;
			// 100    iWidth
			// ---- = ------
			// tHeight  iHeight
			thumbnail.Height=nextImage.Height * 100 / nextImage.Width;
			SetThumbnailLocation(this.PointToClient(new Point(lastX, lastY)));
			thumbnail.Image=nextImage;
		}
	}
}
