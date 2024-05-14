using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KafkaNet;
using KafkaNet.Model;

namespace kafka
{
    public partial class Form1 : Form
    {
        private Consumer _consumer;
        private string _bootstrapServers = "PLAINTEXT://10.11.226.182:9092";
        private string _topicName = "first_topic";
        private bool _isConsumerRunning = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void RefreshConsumer()
        {
            try
            {
                // Initialize Kafka consumer
                var options = new KafkaOptions(new Uri(_bootstrapServers));
                var router = new BrokerRouter(options);
                var consumerOptions = new ConsumerOptions(_topicName, router);

                _consumer = new Consumer(consumerOptions);

                // Start a background task to continuously read messages
                Task.Run(() => StartConsumer());

                _isConsumerRunning = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Kafka consumer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartConsumer()
        {
            try
            {
                foreach (var message in _consumer.Consume())
                {
                    // Update TextBox on the UI thread
                    Invoke(new Action(() =>
                    {
                        textBox1.Text = (Encoding.UTF8.GetString(message.Value) + Environment.NewLine);
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error consuming messages from Kafka: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text == string.Empty)
            {
                MessageBox.Show("Please enter a message", "Message", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string payload = txtMessage.Text.Trim();
            var sendMessage = new Thread(() =>
            {
                KafkaNet.Protocol.Message msg = new KafkaNet.Protocol.Message(payload);
                var options = new KafkaOptions(new Uri(_bootstrapServers)); // Corrected
                var router = new BrokerRouter(options);
                var client = new Producer(router);
                //client = Producer(BrokerRouter(PLAINTEXT://localhost:9092)))
                client.SendMessageAsync(_topicName, new List<KafkaNet.Protocol.Message> { msg }).Wait();

            });
            sendMessage.Start();
            this.Clear();
            textBox1.Text = string.Empty;
            RefreshConsumer();
        }

        void Clear()
        {
            this.txtMessage.Text = string.Empty;
            this.txtMessage.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            RefreshConsumer();
        }
    }
}
