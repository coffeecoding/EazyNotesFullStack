using System;
using System.Collections.Generic;

namespace EazyNotes.Models.POCO
{
    public class CheckListItem : ICloneable
    {
        public bool IsChecked { get; set; }
        public string Text { get; set; }
        public int IndentCount { get; set; }

        public CheckListItem()
        {

        }

        public CheckListItem(string customSerialized)
        {
            string[] parts = customSerialized.Split(',');
            IsChecked = parts[0].Equals("1");
            IndentCount = int.Parse(parts[1]);
            Text = parts[2];
        }

        public CheckListItem(bool isChecked, string text, int indentCount)
        {
            IsChecked = isChecked;
            Text = text;
            IndentCount = indentCount;
        }

        public CheckListItem(char isChecked, string text, int indentCount)
        {
            IsChecked = isChecked == '1' ? true : false;
            Text = text;
            IndentCount = indentCount;
        }

        public static List<CheckListItem> FromArray(string[] items)
        {
            List<string> list = new List<string>(items);
            List<CheckListItem> result = list
                .ConvertAll((line) => {
                    if (line.Length <= 1)
                        return new CheckListItem(false, line, 0);
                    int indentCount = 0;
                    while (line.StartsWith('\t'))
                    {
                        indentCount++;
                        line = line.Remove(0, 1);
                    }
                    bool isNotChecked = line.Substring(0, 2).Equals("- ");

                    return new CheckListItem(!isNotChecked, line[2..], indentCount);
                });
            return result;
        }

        public override int GetHashCode()
        {
            // Note: GetHashCode is NOT used for Equality-testing of this class, therefore this is irrelevant.
            return base.GetHashCode();
        }

        public override bool Equals(object other)
        {
            if (!(other is CheckListItem cli) || other == null)
                return false;
            return IsChecked == cli.IsChecked && Text == cli.Text && IndentCount == cli.IndentCount;
        }

        public string CustomSerialize() => $"{(IsChecked ? 1 : 0)},{IndentCount},{Text}";

        public static List<CheckListItem> CloneList(List<CheckListItem> items)
        {
            return items.ConvertAll(i => i.Clone() as CheckListItem);
        }

        public static bool AreListsEqual(List<CheckListItem> list1, List<CheckListItem> list2)
        {
            if (list1 == null || list2 == null)
                return list1 == list2;
            else if (list1.Count != list2.Count)
                return false;
            for (int i = 0; i < list1.Count; i++)
                if (!list1[i].Equals(list2[i]))
                    return false;
            return true;
        }

        public object Clone()
        {
            return new CheckListItem(IsChecked, Text, IndentCount);
        }

        public override string ToString()
        {
            return new String('\t', IndentCount) + (IsChecked ? "v " : "- ") + Text;
        }
    }
}
