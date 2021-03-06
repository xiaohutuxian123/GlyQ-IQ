﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GlycolyzerGUImvvm.Views
{
    /// <summary>
    /// Interaction logic for OmniFinderPage.xaml
    /// </summary>
    public partial class OmniFinderPage : Page
    {
        public OmniFinderPage()
        {
            InitializeComponent();
            userNumberOf_TextBox.Text = App.omniFinderModel_Save.ParameterNumberOfUserUnits_Int.ToString();
        }

        private void userNumberOf_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox changedTextBox = sender as TextBox;

            string Str = changedTextBox.Text.Trim();
            int Num;
            bool isNum = int.TryParse(Str, out Num);

            if (isNum && Convert.ToInt32(changedTextBox.Text) <= 10)
                App.omniFinderModel_Save.ParameterNumberOfUserUnits_Int = Convert.ToInt32(changedTextBox.Text);
            else if (changedTextBox.Text == "")
                App.omniFinderModel_Save.ParameterNumberOfUserUnits_Int = 0;
            else
            {
                MessageBox.Show("Invalid number. Must be 10 or less.");
                App.omniFinderModel_Save.ParameterNumberOfUserUnits_Int = 0;
            }

            changedTextBox.Text = App.omniFinderModel_Save.ParameterNumberOfUserUnits_Int.ToString();
        }
    }
}
