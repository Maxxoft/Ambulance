using Ambulance.Dependency;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Ambulance.Helper
{
    public class PhoneHelper
    {
        public static string FormattedPhoneNumber(string phoneNumber)
        {
            string formattedNumber = "+";
			if (string.IsNullOrEmpty(phoneNumber)) return formattedNumber;
			for (int i = 0; i < phoneNumber.Length; i++)
            {
				formattedNumber += phoneNumber[i];
				formattedNumber += FormattingNumber(phoneNumber, i+1);
            }
			return formattedNumber;
        }

        public static string FormattingNumber(string num, int Caret)
        {
            if (num[0] != '+')
            {
                if (Caret == 1)
                    return " (";
                if (Caret == 4)
                    return ") ";
                if (Caret == 7)
                    return "-";
            }
            else
            {
                if (Caret == 2)
                    return " (";
                if (Caret == 5)
                    return ") ";
                if (Caret == 8)
                    return "-";
            }
			return string.Empty;
        }

        public static void PhoneNumber_Changed(object sender, TextChangedEventArgs e)
        {
            var TextBox = sender as Entry;
			if (e.NewTextValue == null || e.NewTextValue.Length < 4)
			{
				TextBox.Text = "+7 (";
				return;
			}
			if (e.NewTextValue.Length > 17)
			{
				TextBox.Text = e.NewTextValue.Substring(0, 17);
				return;
			}

            if (TextBox.Text.Length >= 2)
            {
                if (!(TextBox.Text[TextBox.Text.Length - 2] == ')' && TextBox.Text[TextBox.Text.Length - 1] == ' '))
                {
                    TextBox.Text = TextValidation.TextNoSpaces(e.NewTextValue);
                  
                }
            }
            if (e.NewTextValue != null)
                if (e.OldTextValue != null)
                    if (e.NewTextValue.Length < e.OldTextValue.Length)
                    {
                    
                        if (TextBox.Text.Length >= 2)
                        {
                            if (TextBox.Text[TextBox.Text.Length - 2] == ')' && TextBox.Text[TextBox.Text.Length - 1] == ' '
                                ||
                               TextBox.Text[TextBox.Text.Length - 2] == ' ' && TextBox.Text[TextBox.Text.Length - 1] == '(')
                            { TextBox.Text = TextBox.Text.Remove(TextBox.Text.Length - 2); return; }
                            if (TextBox.Text[TextBox.Text.Length - 1] == '-')
                            { TextBox.Text = TextBox.Text.Remove(TextBox.Text.Length - 1); return; }
                       
                        }

                    }
                    else
            if (TextBox.Text != null)
                    {
                    

                        if (TextBox.Text[TextBox.Text.Length - 1] == '.')
                        { TextBox.Text = TextBox.Text.Remove(TextBox.Text.Length - 1); return; }

                        if (TextBox.Text[0] != '+')
                            if (TextBox.Text.Length > 16)
                            { TextBox.Text = e.OldTextValue; return; }
                            else
                                  if (TextBox.Text.Length > 17)
                            {
                                TextBox.Text = e.OldTextValue;
                                return;
                            }

                        var ClearNumber = DigitsPhoneNumber(TextBox.Text).Trim();
                        TextBox.Text = FormattedPhoneNumber(ClearNumber);
                    
                    }

        }


        public static string DigitsPhoneNumber(string phoneNumber)
        {
			if (string.IsNullOrEmpty(phoneNumber)) return null;
			var num = string.Empty;
			var digits = new List<Char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
			foreach (var c in phoneNumber)
			{
				if (digits.Contains(c)) num = num + c;
			}
			return num;
        }

        public static long ValidPhoneNumber(string phoneNumber)
        {
			var num = DigitsPhoneNumber(phoneNumber);
			if (string.IsNullOrEmpty(num) || num.Length != 11) return 0;
            return Convert.ToInt64(num);
        }

		public static string ServicePhoneNumber(long num)
		{
			var number = num.ToString();
			if (number == null || number.Length < 10) return null;
            if (number.Length == 10)
                return "7 (" + number.Substring(0, 3) + ") " + number.Substring(3, 3) + "-" + number.Substring(6, 4);
            if (number.Length == 11)
			    return "7 (" + number.Substring(1, 3) + ") " + number.Substring(4, 3) + "-" + number.Substring(7, 4);
            return null;
		}

		public static string FullPhoneNumber(long num)
		{
			var number = ServicePhoneNumber(num);
			if (number == null) return null;
			return "+" + number;
		}

        public static string GetOwnPhoneNumber()
        {
            try
            {
                return DependencyService.Get<IPhoneManager>().GetOwnPhoneNumber();
            }
            catch(Exception e)
            {
                //GoogleAnalyticsHelper.SendException(e.Message, false);

                return null;
            }
        }
    }
}
