using System;
using System.Drawing;
using System.Windows.Forms;

namespace osu_helper;
/// <summary>
/// Class providing ways to interact with the user.
/// </summary>
public static class PopUp
{
    /// <summary>
    /// Pops up input box asking user to give a value.
    /// </summary>
    /// <param name="title">The title of the window.</param>
    /// <param name="promptText">The text used to prompt the user for action.</param>
    /// <param name="value">The value that will populate the input box as default and passed as reference.</param>
    /// <returns><paramref name="DialogResult"/> containing what the user clicked</returns>
    public static DialogResult InputBox(string title, string promptText, ref string value)
    {
        Form form = new();
        Label label = new();
        TextBox textBox = new();
        Button buttonOk = new();
        Button buttonCancel = new();

        form.Text = title;
        label.Text = promptText;
        textBox.Text = value;

        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;

        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 372, 20);
        buttonOk.SetBounds(228, 72, 75, 23);
        buttonCancel.SetBounds(309, 72, 75, 23);

        label.AutoSize = true;
        textBox.Anchor |= AnchorStyles.Right;
        buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
        form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        DialogResult dialogResult = form.ShowDialog();
        value = textBox.Text;
        return dialogResult;
    }

    /// <summary>
    /// Pops up confirmation that the user wants to do <paramref name="text"/>.
    /// </summary>
    /// <param name="text">Text asking for confirmation that the user wants to do something.</param>
    /// <returns>bool signifying that the user wants to do the action.</returns>
    public static bool Conformation(string text)
    {
        Form form = new();
        Label label = new();
        Button buttonYes = new();
        Button buttonNo = new();

        form.Text = "Are you sure?";
        label.Text = text;

        buttonYes.Text = "Yes";
        buttonNo.Text = "No";
        buttonYes.DialogResult = DialogResult.OK;
        buttonNo.DialogResult = DialogResult.No;

        label.SetBounds(9, 20, 372, 13);
        buttonYes.SetBounds(228, 72, 75, 23);
        buttonNo.SetBounds(309, 72, 75, 23);

        label.AutoSize = true;
        buttonYes.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        buttonNo.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange(new Control[] { label, buttonYes, buttonNo });
        form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonYes;
        form.CancelButton = buttonNo;

        DialogResult dialogResult = form.ShowDialog();

        return dialogResult == DialogResult.OK;
    }
}