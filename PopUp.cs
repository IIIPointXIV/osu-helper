using System;
using System.Drawing;
using System.Windows.Forms;

public static class PopUp
{
    public static DialogResult InputBox(string title, string promptText, ref string value)
    {
        Form form = new Form();
        Label label = new Label();
        TextBox textBox = new TextBox();
        Button buttonOk = new Button();
        Button buttonCancel = new Button();

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
        textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
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

    public static bool Conformation(string text)
    {
        Form form = new Form();
        Label label = new Label();
        Button buttonYes = new Button();
        Button buttonNo = new Button();

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