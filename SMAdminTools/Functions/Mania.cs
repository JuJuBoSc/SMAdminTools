using System;

namespace SMAdminTools.Functions
{
	public class Mania
	{

		public static string StripNadeoColours(string str)
		{
			string newString = String.Empty;

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '$')
				{
					switch (str.ToLower()[i + 1])
					{
						case 's':
						case 't':
						case 'w':
						case 'z':
						case 'g':
						case 'i':
						case 'm':
						case 'n':
						case 'o':
							{
								i++;
								continue;
							}
						case '$':
							{
								newString = newString + '$';
								continue;
							}
					}

					if (((str.ToLower()[i + 1] >= '0') && (str.ToLower()[i + 1] <= '9')) || ((str.ToLower()[i + 1] >= 'a') && (str.ToLower()[i + 1] <= 'f')))
					{
						i += 3;
					}

					continue;
				}

				newString = newString + str[i];
			}

			return newString;
		}

	}
}

