﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AudioBand.Extensions;

namespace AudioBand.Views.Winforms
{
    /// <summary>
    /// Custom button
    /// </summary>
    public class CustomButton : AudioBandControl
    {
        private static readonly Color HoverColor = Color.FromArgb(25, 255, 255, 255);
        private static readonly Color MouseDownColor = Color.FromArgb(15, 255, 255, 255);
        private static readonly Color NoOverlay = Color.FromArgb(0);

        private Image _image;
        private Color _overlayColor = NoOverlay;

        /// <summary>
        /// Gets or sets the button's image.
        /// </summary>
        [Bindable(BindableSupport.Yes)]
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                InvokeRefresh();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _overlayColor = HoverColor;
            InvokeRefresh();
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _overlayColor = NoOverlay;
            InvokeRefresh();
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _overlayColor = MouseDownColor;
            InvokeRefresh();
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _overlayColor = NoOverlay;
            InvokeRefresh();
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            e.Graphics.FillRectangle(new SolidBrush(_overlayColor), e.ClipRectangle);

            if (Image == null)
            {
                return;
            }

            var scaledImage = Image.Scale(e.ClipRectangle.Width, e.ClipRectangle.Height);
            var centerX = (e.ClipRectangle.Width - scaledImage.Width) / 2;
            var centerY = (e.ClipRectangle.Height - scaledImage.Height) / 2;
            graphics.DrawImageUnscaled(scaledImage, new Point(centerX, centerY));
        }
    }
}
