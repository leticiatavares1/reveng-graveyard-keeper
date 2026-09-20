using System.Text;

namespace LazyBearTechnology;

public static class ModsJsonComments
{
	public static string Strip(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return json;
		}
		StringBuilder stringBuilder = new StringBuilder(json.Length);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < json.Length; i++)
		{
			char c = json[i];
			char c2 = ((i + 1 < json.Length) ? json[i + 1] : '\0');
			if (flag3)
			{
				if (c == '\n')
				{
					flag3 = false;
					stringBuilder.Append(c);
				}
				continue;
			}
			if (flag4)
			{
				if (c == '*' && c2 == '/')
				{
					flag4 = false;
					i++;
				}
				continue;
			}
			if (flag)
			{
				stringBuilder.Append(c);
				if (flag2)
				{
					flag2 = false;
					continue;
				}
				switch (c)
				{
				case '\\':
					flag2 = true;
					break;
				case '"':
					flag = false;
					break;
				}
				continue;
			}
			switch (c)
			{
			case '"':
				flag = true;
				stringBuilder.Append(c);
				continue;
			case '/':
				if (c2 == '/')
				{
					flag3 = true;
					i++;
					continue;
				}
				break;
			}
			if (c == '/' && c2 == '*')
			{
				flag4 = true;
				i++;
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
