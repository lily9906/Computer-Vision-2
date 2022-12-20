using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;
using static System.Net.WebRequestMethods;

namespace Computer_Vision_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Bitmap _image;
        private async void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                _image = (Bitmap)Image.FromFile(Openfile.FileName);
                pictureBox1.Image = _image;
            }
            var height = _image.Height;
            var width = _image.Width;
            int x, y;

            var predictionKey = "fb84868c3ea14db9a9d0ca0ca554d192";
            var predictionUrl = "https://southeastasia.api.cognitive.microsoft.com/customvision/v3.0/Prediction/3c9775dd-a5a3-42c6-b6d9-d565c16baf3f/detect/iterations/Iteration2/image";
            var classificationKey = "fb84868c3ea14db9a9d0ca0ca554d192";
            var classificationUrl = "https://southeastasia.api.cognitive.microsoft.com/customvision/v3.0/Prediction/a8f17666-c1d6-49e4-a032-e0928c05a310/classify/iterations/Iteration1/image";


            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);
            var file = _image;
            byte[] imgBytes = filereader.ImageToByte2(_image);

            using (var content = new ByteArrayContent(imgBytes))
            {
                // request API 
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                try
                {
                    var res = await client.PostAsync(predictionUrl, content);
                    var str = await res.Content.ReadAsStringAsync();
                    var myPredictionModel = JsonConvert.DeserializeObject<MyPredictionModel>(str);
                    var predictions = myPredictionModel.predictions;
                    foreach (var prediction in predictions)
                    {
                        //calculate the actual pixel of object found
                        //crop the image using the pixel calculated (create a Rectangle)
                        //convert Bitmap to byte[] again
                        //pass to classification api 
                        //display the result

                        for (x = 0; x < _image.Height;)        
                        {
                            for (y = 0; y < _image.Width;)            
                            {
                                Color pixelColor = _image.GetPixel(x, y);
                                Color newColor = Color.FromArgb(pixelColor.R, 0, 0);
                                _image.SetPixel(x, y, newColor);
                            }
                        }


                        if (prediction.probability > 0.7 && prediction.tagName == "People")
                        {
                            label1.Text = "Yes, It is people";
                            label2.Text = myPredictionModel.created.ToString();


                        }

                        else if (prediction.probability > 0.7 && prediction.tagName == "Cat")
                        {
                            label1.Text = "It is cat";

                        }

                        else if (prediction.probability > 0.5 && prediction.tagName == "Dog")
                        {
                            label1.Text = "Yes, it is dog.";
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
        }

        Crop filter = new Crop(new Rectangle(75, 75, 320, 240));
        private Bitmap newImage;

        public Filter()
        {
            Bitmap newImage = filter.Apply(_image);
        }

        public class filereader
        {
            public static byte[] ImageToByte2(Image img)
            {
                using (var stream = new MemoryStream())
                {
                    img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.ToArray();
                }
            }

            /*public byte[] imagetobytearray(system.drawing.image imagein)
            {
                using (var ms = new memorystream())
                {
                    imagein.save(ms, imagein.rawformat);
                    return ms.toarray();
                }
           }*/

            /*public static byte[] readfully(Stream input)
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = input.read(buffer, 0, buffer.length)) > 0)
                    {
                        ms.write(buffer, 0, read);
                    }
                    return ms.toarray();
                }
            }*/
        }
    }
}

