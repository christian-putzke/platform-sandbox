using TMPro;
using UnityEngine;

namespace plasa.debug
{
	public class ui : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		private static string _textInternal;

		public static void WriteText(string text, bool clear = false)
		{
			if (clear) {
				_textInternal = "";
			}

			_textInternal += $"{text}\n";
		}

		private void LateUpdate()
		{
			if (_textInternal != null && _textInternal != "") {
				_text.text = _textInternal;
			}
		}
	}
}
