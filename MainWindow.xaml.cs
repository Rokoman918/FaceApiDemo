using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceApiDemo
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IFaceServiceClient _faceServiceCliente = new FaceServiceClient("458bf928c4a0469dae77f2cbbe227c60", "https://southcentralus.api.cognitive.microsoft.com/face/v1.0");

        public MainWindow()
        {
            InitializeComponent();
        }

        string filePath2 = string.Empty;
        string filePath = string.Empty;
       
        IEnumerable<Guid> faceid1;
        IEnumerable<Guid> faceid2;
        

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lblResult.Content = "";
                var openFrame = new Microsoft.Win32.OpenFileDialog { Filter = "JPEG Image(*.jpg)|*.jpg" };
                var result = openFrame.ShowDialog(this);
                if (!(bool)result)
                {
                    return;
                }
                filePath = openFrame.FileName;
                var fileUri = new Uri(filePath);
                var bitMapSource = new BitmapImage();
                bitMapSource.BeginInit();
                bitMapSource.CacheOption = BitmapCacheOption.None;
                bitMapSource.UriSource = fileUri;
                bitMapSource.EndInit();
                FaceImage.Source = bitMapSource;


                var openFrame2 = new Microsoft.Win32.OpenFileDialog { Filter = "JPEG Image(*.jpg)|*.jpg" };
                var result2 = openFrame.ShowDialog(this);
                if (!(bool)result)
                {
                    return;
                }
                filePath2 = openFrame.FileName;
                var fileUri2 = new Uri(filePath2);
                var bitMapSource2 = new BitmapImage();
                bitMapSource2.BeginInit();
                bitMapSource2.CacheOption = BitmapCacheOption.None;
                bitMapSource2.UriSource = fileUri2;
                bitMapSource2.EndInit();
                FaceImage2.Source = bitMapSource2;

                BeginingDetection(filePath, 1, bitMapSource);
                BeginingDetection(filePath2, 2, bitMapSource2);
            }
            catch (Exception ex)
            {

                lblResult.Content = ex.Message;
            }
            

        }

        private async void BeginingDetection(String filePath, int numFace, BitmapImage bitMapSource)
        {
            lblResult.Content = "Detectando Rostro " + numFace;

            Title = "Detectando Rostro " + numFace;
            FaceRectangle[] faceFound = await DetectTheFaces(filePath, numFace);
            Title = $"Se encontraron {faceFound.Length} rostro(s)";
            lblResult.Content = $"Se encontraron {faceFound.Length + numFace  } rostro(s)";


            if (faceFound.Length <= 0) return;
            var drwVisual = new DrawingVisual();
            var drwContext = drwVisual.RenderOpen();
            drwContext.DrawImage(bitMapSource, new Rect(0, 0, bitMapSource.Width, bitMapSource.Height));
            var dpi = bitMapSource.DpiX;
            var resizeFactor = 96 / dpi;

            foreach (var faceRect in faceFound)
            {
                drwContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue, 5),
               new Rect(faceRect.Left * resizeFactor, faceRect.Top * resizeFactor, faceRect.Width * resizeFactor,
                faceRect.Height * resizeFactor));

            }
            drwContext.Close();
            var renderToImageCtrl = new RenderTargetBitmap((int)(bitMapSource.PixelWidth * resizeFactor), (int)(bitMapSource.PixelHeight * resizeFactor), 96, 96, PixelFormats.Pbgra32);
            renderToImageCtrl.Render(drwVisual);
            if(numFace == 1) FaceImage.Source = renderToImageCtrl;
            if (numFace == 2) FaceImage2.Source = renderToImageCtrl;

        }
        private async Task<FaceRectangle[]> DetectTheFaces(string filePath, int numFace)
        {

            try
            {
                var imgStream = File.OpenRead(filePath);
                var faces = await _faceServiceCliente.DetectAsync(imgStream);
                var faceRectangles = faces.Select(Face => Face.FaceRectangle);
                if (numFace == 1) faceid1 = faces.Select(Face => Face.FaceId);
                if (numFace == 2) faceid2 = faces.Select(Face => Face.FaceId);

                return faceRectangles.ToArray();

            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        private async void GoToVerify_Click(object sender, RoutedEventArgs e)

        {
            try
            {
                List<Guid> a;
                List<Guid> b;
                a = faceid1.ToList<Guid>();

                b = faceid2.ToList<Guid>();

                if ((a[0] != null) && (b[0] != null))
                {
                    var result = await VerifyTheFaces(a, b);
                    if (result.IsIdentical == true)
                    {
                        lblResult.Content = "Es la Misma Persona, nivel de coincidencia " + result.Confidence;
                    }
                    else
                    {
                        lblResult.Content = "No parece ser la misma persona,  Nivel de coincidencia" + result.Confidence;
                    }
                    
                }
                else
                { MessageBox.Show("No se puede realizar el proceso de verificación falta faceId"); }
            }
            catch (Exception ex)
            {

                lblResult.Content=  ex.Message;
            }

          
        }

        private async Task<VerifyResult> VerifyTheFaces(List<Guid>  a, List<Guid> b)
        {

            try
            {

                VerifyResult x = await _faceServiceCliente.VerifyAsync(a[0], b[0]);

                return x;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
