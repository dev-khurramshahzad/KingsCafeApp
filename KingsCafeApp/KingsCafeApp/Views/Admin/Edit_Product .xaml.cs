using Firebase.Database.Query;
using KingsCafeApp.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KingsCafeApp.Views.Admin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Edit_Product : ContentPage
    {

        private MediaFile _mediaFile;
        public static string PicPath = "category_picker.png";
        public static tblFoodProduct item = null;
        public static bool IsNewPicSelected = false;
        public static List<tblFoodCategory> firebaseList = null;
        public Edit_Product(tblFoodProduct p)
        {
            InitializeComponent();
            LoadingInd.IsRunning = true;
            LoadData();
            item = p;
            lblCategory.Text = firebaseList.Where(x => x.FOOD_CATEGORIES_ID == p.FOOD_CATEGORIES_FID).FirstOrDefault().FOOD_CATEGORIES_NAME;
            txtProName.Text = p.FOOD_PRODUCT_NAME;
            txtProPrice.Text = p.FOOD_PRODUCT_PRICE.ToString();
            PreviewPic.Source = p.FOOD_PRODUCT_PICTURE;
            LoadingInd.IsRunning = false;
        }

        async void LoadData()
        {
            firebaseList = (await App.firebaseDatabase.Child("tblFoodCategory").OnceAsync<tblFoodCategory>()).Select(x => new tblFoodCategory
            {
                FOOD_CATEGORIES_ID = x.Object.FOOD_CATEGORIES_ID,
                FOOD_CATEGORIES_NAME = x.Object.FOOD_CATEGORIES_NAME,
                FOOD_CATEGORIES_PICTURE = x.Object.FOOD_CATEGORIES_PICTURE,
            }).ToList();

            var refinedList = firebaseList.Select(x => x.FOOD_CATEGORIES_NAME).ToList();
            ddlCat.ItemsSource = refinedList;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            try
            {
                var response = await DisplayActionSheet("Select Image Source", "Close", "", "From Gallery", "From Camera");
                if (response == "From Camera")
                {
                    await CrossMedia.Current.Initialize();
                    if (!CrossMedia.Current.IsTakePhotoSupported)
                    {
                        await DisplayAlert("Error", "Phone is not Take Photo Supported", "OK");
                        return;
                    }

                    var mediaOptions = new StoreCameraMediaOptions()
                    {
                        PhotoSize = PhotoSize.Medium
                    };

                    var SelectedImg = await CrossMedia.Current.TakePhotoAsync(mediaOptions);

                    if (SelectedImg == null)
                    {
                        await DisplayAlert("Error", "Error Taking Image...", "OK");
                        return;
                    }

                    _mediaFile = SelectedImg;
                    PicPath = SelectedImg.Path;
                    PreviewPic.Source = SelectedImg.Path;


                }
                if (response == "From Gallery")
                {
                    await CrossMedia.Current.Initialize();
                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        await DisplayAlert("Error", "Phone is not Pick Photo Supported", "OK");
                        return;
                    }

                    var mediaOptions = new PickMediaOptions()
                    {
                        PhotoSize = PhotoSize.Medium
                    };

                    var SelectedImg = await CrossMedia.Current.PickPhotoAsync(mediaOptions);

                    if (SelectedImg == null)
                    {
                        await DisplayAlert("Error", "Error Picking Image...", "OK");
                        return;
                    }

                    _mediaFile = SelectedImg;
                    PicPath = SelectedImg.Path;
                    PreviewPic.Source = SelectedImg.Path;


                }
                IsNewPicSelected = true;
            }
            catch (Exception ex)
            {
                LoadingInd.IsRunning = false;
                await DisplayAlert("Message", "Something Went wrong \n" + ex.Message, "OK");

            }
        }

        private async void btnPro_Clicked(object sender, EventArgs e)
        {
            try
            {


                if (string.IsNullOrEmpty(txtProName.Text) || string.IsNullOrEmpty(txtProPrice.Text))
                {
                    await DisplayAlert("Error", "Please fill required fields and try again", "Ok");
                    return;
                }
                if (ddlCat.SelectedItem == null)
                {
                    await DisplayAlert("Error", "Please Select required Category and try again", "Ok");
                    return;
                }

                LoadingInd.IsRunning = true;

                string img = item.FOOD_PRODUCT_PICTURE;

                if (IsNewPicSelected == true)
                {
                    var StoredImageURL = await App.FirebaseStorage
                    .Child("FOOD_CATEGORIES_PICTURE")
                    .Child(item.FOOD_PRODUCT_ID.ToString() + "_" + txtProName.Text + ".jpg")
                    .PutAsync(_mediaFile.GetStream());

                    img = StoredImageURL;

                }

                var SelectedCatID = firebaseList.Where(x => x.FOOD_CATEGORIES_NAME == lblCategory.Text).FirstOrDefault().FOOD_CATEGORIES_ID;

                var OldProduct = (await App.firebaseDatabase.Child("tblFoodProduct").OnceAsync<tblFoodProduct>()).FirstOrDefault(x => x.Object.FOOD_PRODUCT_ID == item.FOOD_PRODUCT_ID);

                OldProduct.Object.FOOD_PRODUCT_ID = item.FOOD_PRODUCT_ID;
                OldProduct.Object.FOOD_PRODUCT_NAME = txtProName.Text;
                OldProduct.Object.FOOD_CATEGORIES_FID = SelectedCatID;
                OldProduct.Object.FOOD_PRODUCT_PICTURE = img;


                await App.firebaseDatabase.Child("tblFoodProduct").Child(OldProduct.Key).PutAsync(OldProduct.Object);
                LoadingInd.IsRunning = false;
                await DisplayAlert("Success", "Product Updated successfully", "Ok");
                await Navigation.PopAsync();

            }
            catch (Exception ex)
            {
                LoadingInd.IsRunning = false;
                await DisplayAlert("Error", "Something went wrong, please try again later. \nError: " + ex.Message, "Ok");
            }
        }

        private void ddlCat_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblCategory.Text = ddlCat.SelectedItem.ToString();
        }

        private void TapGestureRecognizer_Tapped_1(object sender, EventArgs e)
        {
            ddlCat.IsVisible = true;
        }
    }
}