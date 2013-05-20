using System.Windows;
using System.Windows.Controls;

namespace TpAdministration.Controls
{
    /// <summary>
    ///     A combination watermarked textbox and label with the entered value bolded
    ///     Allows you to have a watermark textbow without a seperate label control, and after the value is entered
    ///     have some custom text displayed that includes the value, rather than having a textbox with a value entered
    ///     but no context as to what the value is meant to represent
    /// </summary>
    public class LabeledTextBox : TextBox
    {
        /// <summary>
        ///     The watermark dependency property, the watermark that displays in the textbox
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark",
            typeof (string),
            typeof (LabeledTextBox),
            new PropertyMetadata(string.Empty));


        /// <summary>
        ///     Has Text Dependency Property Key
        /// </summary>
        private static readonly DependencyPropertyKey HasTextPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasText",
            typeof (bool),
            typeof (LabeledTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     HasText dependency property
        /// </summary>
        public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;


        /// <summary>
        ///     Label dependency property, the Label which will be displayed when the texbox has a value
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof (string),
                                                                                              typeof (LabeledTextBox));

        /// <summary>
        ///     Initializes the <see cref="LabeledTextBox" /> class.
        /// </summary>
        static LabeledTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (LabeledTextBox),
                                                     new FrameworkPropertyMetadata(typeof (LabeledTextBox)));

            //hook up the Text Changed event
            TextProperty.OverrideMetadata(
                typeof (LabeledTextBox),
                new FrameworkPropertyMetadata(TextPropertyChanged));
        }

        /// <summary>
        ///     Gets or sets the watermark.
        /// </summary>
        /// <value>
        ///     The watermark.
        /// </value>
        public string Watermark
        {
            get { return (string) GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance has text.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has text; otherwise, <c>false</c>.
        /// </value>
        public bool HasText
        {
            get { return (bool) GetValue(HasTextProperty); }
        }

        /// <summary>
        ///     Gets or sets the label property
        /// </summary>
        /// <value>
        ///     The label.
        /// </value>
        public string Label
        {
            get { return (string) GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }


        /// <summary>
        ///     Handles when the textbox text property fires
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">
        ///     The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.
        /// </param>
        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var ltb = (LabeledTextBox) sender;

            bool actuallyHasText = ltb.Text.Length > 0;
            if (actuallyHasText != ltb.HasText)
            {
                ltb.SetValue(HasTextPropertyKey, actuallyHasText);
            }
        }
    }
}