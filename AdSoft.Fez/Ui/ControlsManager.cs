namespace AdSoft.Fez.Ui
{
    using System.Collections;

    using AdSoft.Fez.Ui.Interfaces;

    public class ControlsManager: IUiWorker
    {
        public ArrayList Controls { get; private set; }

        public ControlsManager()
        {
            Controls = new ArrayList();
        }
        
        public Control Add(Control control)
        {
            Controls.Add(control);
            return control;
        }

        public void Show()
        {
            Control focused = null;

            for (int i = 0; i < Controls.Count; i++)
            {
                var control = Controls[i] as Control;

                if (control == null || !control.IsVisible)
                {
                    continue;
                }

                control.Show();

                if (control.IsFocused)
                {
                    focused = control;
                }
            }

            if (focused != null)
            {
                focused.Focus();
            }
        }


        public void Start()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                var uiWorker = Controls[i] as IUiWorker;

                if (uiWorker == null)
                {
                    continue;
                }

                uiWorker.Start();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < Controls.Count; i++)
            {
                var uiWorker = Controls[i] as IUiWorker;

                if (uiWorker == null)
                {
                    continue;
                }

                uiWorker.Stop();
            }
        }
    }
}
