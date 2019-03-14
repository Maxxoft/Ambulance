using System;
using Xamarin.Forms;

namespace Ambulance.Helper
{
    public static class TextValidation
    {
        public static void TextOnlyLetters_Changed(object sender, TextChangedEventArgs e)
        {
            var TextBox = sender as Entry;

            if (e.NewTextValue != null)
                if (e.NewTextValue.Length > 0)
                    if (!Char.IsLetter(e.NewTextValue[e.NewTextValue.Length - 1]))
                        TextBox.Text = TextBox.Text.Remove(TextBox.Text.Length - 1);
        }
        public static void TextOnlyDigits(object sender, TextChangedEventArgs e)
        {
            var TextBox = sender as Entry;

            if (e.NewTextValue != null)
                if (e.NewTextValue.Length > 0)
                    if (!Char.IsDigit(e.NewTextValue[e.NewTextValue.Length - 1]))
                        TextBox.Text = TextBox.Text.Remove(TextBox.Text.Length - 1);
        }

        public static void TextOnlyDigitsWithSpaces(object sender, TextChangedEventArgs e)
        {
            var TextBox = sender as Entry;

            if (e.NewTextValue != null)
                if (e.NewTextValue.Length > 0)
                    if(e.NewTextValue[e.NewTextValue.Length - 1]  != ' ')
                    if (!Char.IsDigit(e.NewTextValue[e.NewTextValue.Length - 1]))
                        TextBox.Text = TextBox.Text.Remove(TextBox.Text.Length - 1);
        }

        public static string TextNoSpaces(string NewTextValue)
        {
            if (NewTextValue != null)
                if (NewTextValue.Length > 0)
                    if (NewTextValue[NewTextValue.Length - 1] == ' ')
                       return  NewTextValue.Remove(NewTextValue.Length - 1);
            return NewTextValue;
        }
    }
}
