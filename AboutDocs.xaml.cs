using Hosreg1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Hosreg1
{
    public partial class DoctorReviewsWindow : Window
    {
        private List<ReviewResponse> _reviews;

        // Add constructor that accepts reviews
        public DoctorReviewsWindow(List<ReviewResponse> reviews)
        {
            InitializeComponent();
            _reviews = reviews;
            LoadReviews();
        }

        private void LoadReviews()
        {
            // Group reviews by doctor
            var doctorReviews = _reviews
                .GroupBy(r => r.Doctor)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Set doctors as the source for the combo box
            DoctorsComboBox.ItemsSource = doctorReviews.Keys.ToList();
        }

        private void DoctorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorsComboBox.SelectedItem is Doctor selectedDoctor)
            {
                // Filter reviews for selected doctor
                var doctorReviews = _reviews
                    .Where(r => r.Doctor.Id == selectedDoctor.Id)
                    .OrderByDescending(r => r.Date)
                    .ToList();

                ReviewsListView.ItemsSource = doctorReviews;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}