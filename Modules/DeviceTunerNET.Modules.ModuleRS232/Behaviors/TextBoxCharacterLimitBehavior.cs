using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace DeviceTunerNET.Modules.ModuleRS232.Behaviors
{
    public class TextBoxCharacterLimitBehavior
    {
        public class CharacterLimitBehavior : Behavior<TextBox>
        {
            private const int MinValue = 1;
            private const int MaxValue = 127;

            protected override void OnAttached()
            {
                base.OnAttached();
                AssociatedObject.PreviewTextInput += OnPreviewTextInput;
                DataObject.AddPastingHandler(AssociatedObject, OnPaste);
            }

            protected override void OnDetaching()
            {
                base.OnDetaching();
                AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
                DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
            }

            private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
            {
                if (!IsValidInput(e.Text))
                {
                    e.Handled = true;
                }
            }

            private bool IsValidInput(string text)
            {
                if (int.TryParse(AssociatedObject.Text + text, out int value))
                {
                    return value >= MinValue && value <= MaxValue;
                }

                return false;
            }

            private void OnPaste(object sender, DataObjectPastingEventArgs e)
            {
                if (e.DataObject.GetDataPresent(DataFormats.Text))
                {
                    string pastedText = e.DataObject.GetData(DataFormats.Text) as string;

                    if (!IsValidInput(pastedText))
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    e.CancelCommand();
                }
            }
        }
    }
}
