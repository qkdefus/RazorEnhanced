﻿using System;
using System.Windows.Forms;


namespace RazorEnhanced.UI
{
	public partial class EnhancedBuyAgentManualAdd : Form
	{
		private const string m_Title = "Enhanced Buy Manual Add Item";

		public EnhancedBuyAgentManualAdd()
		{
			InitializeComponent();
			MaximizeBox = false;

			this.Text = m_Title;
		}

		private void EnhancedBuyManualAdd_Load(object sender, EventArgs e)
		{
			tName.Text = "New Item";
			tAmount.Text = "999";
			tGraphics.Text = "0x0000";
			tHue.Text = "0x0000";
		}

		private void bClose_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void bAddItem_Click(object sender, EventArgs e)
		{
			bool fail = false;
			int graphics = 0;
			int amount = 0;
			int hue = 0;
			if (tName.Text == null)
			{
				MessageBox.Show("Item name is not valid.",
				"Item name Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
				fail = true;
			}

			try
			{
				graphics = Convert.ToInt32(tGraphics.Text, 16);
			}
			catch
			{
				MessageBox.Show("Item Graphics is not valid.",
				"Item Graphics Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
				fail = true;
			}

			try
			{

				amount = Convert.ToInt32(tAmount.Text);
			}
			catch
			{
				MessageBox.Show("Item Amount is not valid.",
				"Item Amount Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation,
				MessageBoxDefaultButton.Button1);
				fail = true;
			}

			if (tHue.Text == "-1")
				hue = -1;
			else
			{
				try
				{

					hue = Convert.ToInt32(tHue.Text, 16);
				}
				catch
				{
					MessageBox.Show("Item Color is not valid.",
					"Item Color Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1);
					fail = true;
				}
			}

			if (!fail)
			{
				RazorEnhanced.BuyAgent.AddItemToList(tName.Text, graphics, amount, hue);
				this.Close();
			}

		}
	}
}
