using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace KingsCafeApp.Models
{
    public class tblFoodProduct
    {
        [PrimaryKey, AutoIncrement]
        public int FOOD_PRODUCT_ID { get; set; }
        public string FOOD_PRODUCT_NAME { get; set; }
        public decimal FOOD_PRODUCT_PRICE { get; set; }
        public string FOOD_PRODUCT_PICTURE { get; set; }

        public int Quantity { get; set; }
        public int FOOD_CATEGORIES_FID { get; set; }

        public string Detail
        {
            get
            {
                return string.Format("Rs. {0} | Cat: {1} Qty | {2} ", FOOD_PRODUCT_PRICE, FOOD_CATEGORIES_FID, Quantity); 
            }
        }

    }
}