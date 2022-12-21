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
        private Bitmap _image, newimage;
       
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



                        if (prediction.probability < 0.7)
                            continue;


                        var Left = Convert.ToInt32(prediction.boundingBox.left * width);
                        var Top = Convert.ToInt32(prediction.boundingBox.top * height);
                        var predictionWidth = Convert.ToInt32(prediction.boundingBox.width * width);
                        var predictionHeight = Convert.ToInt32(prediction.boundingBox.height * height);

                        Rectangle rekt = new Rectangle(Left, Top, predictionWidth, predictionHeight);
                        IFilter imgFilter = new Crop(rekt);
                        var croppedImage = imgFilter.Apply(_image);
                        pictureBox2.Image = croppedImage;

                        if (prediction.probability > 0.7 && prediction.tagName == "People")
                        {
                            label1.Text = "Yes, It is people";
                            label2.Text = myPredictionModel.created.ToString();

                        }

                        else if (prediction.probability > 0.5 && prediction.tagName == "Cat")
                        {
                            label1.Text = "It is cat";

                        }

                        else if (prediction.probability > 0.5 && prediction.tagName == "Dog")
                        {
                            label1.Text = "it is dog.";
                        }


                    }

                }

                catch (Exception ex)
                {

                }

                HttpClient client2 = new HttpClient();
                client2.DefaultRequestHeaders.Add("Classification-Key", classificationKey);
                var file2 = _image;
                byte[] imgBytes2 = filereader.ImageToByte2(_image);

                using (var content2 = new ByteArrayContent(imgBytes2))
                {
                    // request API 
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    try
                    {
                        var res2 = await client.PostAsync(classificationUrl, content2);
                        var str2 = await res2.Content.ReadAsStringAsync();
                        var myPredictionModel2 = JsonConvert.DeserializeObject<MyPredictionModel>(str2);
                        var predictions = myPredictionModel2.predictions;
                        foreach (var prediction2 in predictions)
                        {
                            if (prediction2.probability > 0.7 && prediction2.tagName == "Fall")
                            {
                                label3.Text = "Yes,he/she is falling.";
                                label2.Text = myPredictionModel2.created.ToString();

                            }

                            else if (prediction2.probability > 0.5 && prediction2.tagName == "Stand")
                            {
                                label3.Text = "This posture is standing.";

                            }

                            else if (prediction2.probability > 0.5 && prediction2.tagName == "Sit")
                            {
                                label3.Text = "This posture is sitting.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
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
        }
    }
}

