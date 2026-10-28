using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class AvatarForms : Form
{
    private readonly System.Windows.Forms.Timer animacao;
    private readonly System.Windows.Forms.Timer timerPiscada;
    private PictureBox imagemRobo;

    private bool bocaAberta = false;
    private bool falando = false;
    private bool olhosFechados = false;

    //private DesenharEfeitos;
    private float energia = 0;
    private readonly Random random = new Random();

    private const int WM_HOTKEY = 0x0312;
    private const int ID_ATALHO = 9000;

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_NOREPEAT = 0x4000;
    private const uint VK_M = 0x4D;


    public AvatarForms()
    {
        Text = "Assistente Virtual Pitoco";
        Size = new Size(220, 280);
        // StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(12, 20, 40);

        // Manter o avatar acima das outras janelas
        this.TopMost = true;

        // Remover a borda da janela (opcional)
        this.FormBorderStyle = FormBorderStyle.None;

        // Posicionar no canto inferior direito
        var area = Screen.PrimaryScreen!.WorkingArea;

        this.StartPosition = FormStartPosition.Manual;
        this.Location = new Point(
            area.Right - this.Width - 20,
            area.Bottom - this.Height - 20
        );

        DoubleBuffered = true;

        animacao = new System.Windows.Forms.Timer();
        animacao.Interval = 160;

        animacao.Tick += (s, e) =>
        {
            if (falando)
            {
                bocaAberta = !bocaAberta;
                Invalidate();
            }
        };

        imagemRobo = new PictureBox();

        imagemRobo.Dock = DockStyle.Fill;
        imagemRobo.SizeMode = PictureBoxSizeMode.Zoom;
        imagemRobo.BackColor = Color.FromArgb(12, 20, 40);

        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "images",
            "robo.png"
        );

        if (File.Exists(caminho))
        {
            imagemRobo.Image = Image.FromFile(caminho);
        }
        else
        {
            MessageBox.Show("Imagem não encontrada: " + caminho);
        }

        Controls.Add(imagemRobo);
        imagemRobo.BringToFront();

        animacao = new System.Windows.Forms.Timer();
        animacao.Interval = 120;
        animacao.Tick += (s, e) =>
        {
            energia += 0.15f;

            if (falando)
                bocaAberta = !bocaAberta;

            imagemRobo.Invalidate();
        };

        timerPiscada = new System.Windows.Forms.Timer();
        timerPiscada.Interval = random.Next(2500, 5000);


        //

        animacao = new System.Windows.Forms.Timer();
        animacao.Interval = 120;

        animacao.Tick += (s, e) =>
        {
            energia += 0.15f;

            if (falando)
                bocaAberta = !bocaAberta;

            imagemRobo.Invalidate();
        };

        timerPiscada = new System.Windows.Forms.Timer();
        timerPiscada.Interval = random.Next(2500, 5000);

        timerPiscada.Tick += (s, e) =>
        {
            olhosFechados = true;
            imagemRobo.Invalidate();

            var pausa = new System.Windows.Forms.Timer();
            pausa.Interval = 150;

            pausa.Tick += (sender, args) =>
            {
                pausa.Stop();
                pausa.Dispose();

                olhosFechados = false;
                timerPiscada.Interval = random.Next(2500, 5000);
                imagemRobo.Invalidate();
            };

            pausa.Start();
        };

        imagemRobo.Paint += DesenharEfeitos;
        timerPiscada.Start();
        animacao.Start();
    }
    private void DesenharEfeitos(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode =
            System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int largura = imagemRobo.ClientSize.Width;
        int altura = imagemRobo.ClientSize.Height;

        if (largura <= 0 || altura <= 0)
            return;

        // Efeito de energia ao redor do robô
        int margem = 8 + (int)(Math.Abs(Math.Sin(energia)) * 8);

        using (Pen brilho = new Pen(
    Color.FromArgb(170, 0, 200, 255), 3))
        {
            using (var caminho = new System.Drawing.Drawing2D.GraphicsPath())
            {
                int x = margem;
                int y = margem;
                int w = largura - margem * 2;
                int h = altura - margem * 2;
                int raio = 24;

                if (w > 0 && h > 0)
                {
                    caminho.AddArc(x, y, raio, raio, 180, 90);
                    caminho.AddArc(x + w - raio, y, raio, raio, 270, 90);
                    caminho.AddArc(x + w - raio, y + h - raio, raio, raio, 0, 90);
                    caminho.AddArc(x, y + h - raio, raio, raio, 90, 90);
                    caminho.CloseFigure();

                    g.DrawPath(brilho, caminho);
                }
            }
        }
        // Coordenadas proporcionais: ajuste conforme sua imagem
        float centroX = largura * 0.50f;
        float olhosY = altura * 0.250f;
        float distanciaOlhos = largura * 0.10f;

        using Brush ciano = new SolidBrush(Color.Cyan);
        using Pen linhaOlhos = new Pen(Color.Cyan, 4);

        if (olhosFechados)
        {
            g.DrawLine(
                linhaOlhos,
                centroX - distanciaOlhos - 12,
                olhosY,
                centroX - distanciaOlhos + 12,
                olhosY);

            g.DrawLine(
                linhaOlhos,
                centroX + distanciaOlhos - 12,
                olhosY,
                centroX + distanciaOlhos + 12,
                olhosY);
        }
        else
        {
            float raio = largura * 0.017f;

            g.FillEllipse(
                ciano,
                centroX - distanciaOlhos - raio,
                olhosY - raio,
                raio * 2,
                raio * 2);

            g.FillEllipse(
                ciano,
                centroX + distanciaOlhos - raio,
                olhosY - raio,
                raio * 2,
                raio * 2);
        }

        // Boca animada
        float bocaX = largura * 0.4f;
        float bocaY = altura * 0.3f;
        float bocaLargura = largura * 0.20f;

        using Brush boca = new SolidBrush(Color.DeepSkyBlue);

        if (falando && bocaAberta)
        {
            g.FillEllipse(
                boca,
                bocaX,
                bocaY,
                bocaLargura,
                altura * 0.045f);
        }
        else
        {
            using Pen linhaBoca = new Pen(Color.DeepSkyBlue, 3);

            g.DrawLine(
                linhaBoca,
                bocaX,
                bocaY + 5,
                bocaX + bocaLargura,
                bocaY + 5);
        }


    }

    public void StartFala()
    {

        if (InvokeRequired)
        {
            BeginInvoke(new Action(StartFala));
            return;
        }

        falando = true;
        animacao.Start();
        imagemRobo.Invalidate();
    }

    public void StopFala()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(StopFala));
            return;
        }

        falando = false;
        bocaAberta = false;
        imagemRobo.Invalidate();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        animacao.Stop();
        timerPiscada.Stop();

        animacao.Dispose();
        timerPiscada.Dispose();

        if (imagemRobo.Image != null)
            imagemRobo.Image.Dispose();

        base.OnFormClosed(e);
    }


    public void HideAvatar()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(HideAvatar));
            return;
        }

        Hide();
    }


    public void ShowAvatar()
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(ShowAvatar));
            return;
        }

        Show();

        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;

        TopMost = true;
        Activate();
    }




}