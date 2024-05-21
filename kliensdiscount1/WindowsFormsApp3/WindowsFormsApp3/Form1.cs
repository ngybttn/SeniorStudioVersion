using Hotcakes.CommerceDTO.v1.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
	public partial class Form1 : Form
	{
		private readonly Hotcakes.CommerceDTO.v1.Client.Api _proxy;
		private List<ProductDTO> _allProducts;
		private Dictionary<ProductDTO, decimal> originalPrices = new Dictionary<ProductDTO, decimal>();

		public Form1()
		{
			InitializeComponent();

			string url = "http://20.234.113.211:8103";
			string key = "1-9777f99c-010f-461a-9d1c-c851faf6dcae";

			_proxy = new Hotcakes.CommerceDTO.v1.Client.Api(url, key);

			LoadProducts();
			
		}

		private void LoadProducts()
		{
			Hotcakes.CommerceDTO.v1.ApiResponse<List<ProductDTO>> response = _proxy.ProductsFindAll();

			if (response != null && response.Content != null)
			{
				_allProducts = response.Content;

				
				dataGridView1.DataSource = _allProducts.Select(p => new { p.ProductName, SitePrice = Math.Round(p.SitePrice, 2) }).ToList();

				
				dataGridView1.Columns[0].HeaderText = "Product Name";
				dataGridView1.Columns[1].HeaderText = "Site Price";

				
				dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			}
			else
			{
				MessageBox.Show("Failed to fetch products or the response is empty. Check your API configuration.");
			}
		}

		private void buttonApply_Click(object sender, EventArgs e)
		{
			
			DataGridViewSelectedRowCollection selectedRows = dataGridView1.SelectedRows;

			if (selectedRows.Count == 0)
			{
				MessageBox.Show("Please select at least one product to apply the discount.");
				return;
			}

			
			int discountPercentage = int.Parse(comboBoxDiscount.SelectedItem.ToString().Replace("%", ""));

			
			foreach (DataGridViewRow row in selectedRows)
			{
				ProductDTO product = _allProducts[row.Index];

				
				if (!originalPrices.ContainsKey(product))
				{
					originalPrices.Add(product, product.SitePrice);
				}

				
				decimal discountMultiplier = (100 - discountPercentage) / 100.0m;
				product.SitePrice *= discountMultiplier;

				
				try
				{
					var response = _proxy.ProductsUpdate(product);

					if (response != null)
					{
						MessageBox.Show("Discount applied successfully.");
					}
					else
					{
						MessageBox.Show("Failed to apply discount.");
						
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"An error occurred: {ex.Message}");
				}
			}

			// Refresh DataGridView to reflect changes
			LoadProducts();
		}


		private void buttonUndo_Click(object sender, EventArgs e)
		{
			
			string selectedDiscount = comboBoxDiscount.SelectedItem.ToString().Trim('%');
			decimal discountPercentage = Convert.ToDecimal(selectedDiscount) / 100;

			foreach (DataGridViewRow row in dataGridView1.SelectedRows)
			{
				ProductDTO product = _allProducts[row.Index];

				
				product.SitePrice /= (1 - discountPercentage);

				try
				{
					var response = _proxy.ProductsUpdate(product);

					if (response != null)
					{
						MessageBox.Show("Discount reverted successfully.");
					}
					else
					{
						MessageBox.Show("Failed to revert discount.");
						
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"An error occurred: {ex.Message}");
				}
			}

			// Refresh DataGridView to reflect changes
			LoadProducts();
		}

		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{

		}
	}
}
