using System;
using System.Reflection;

namespace Ambulance.Extensions
{
	public static class EnumExtensions
	{
		// This extension method is broken out so you can use a similar pattern with 
		// other MetaData elements in the future. This is your base method for each.
		public static T GetAttribute<T>(this Enum value) where T : Attribute
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			return (T)type.GetRuntimeField(name).GetCustomAttribute(typeof(T));
		}
	}
}
