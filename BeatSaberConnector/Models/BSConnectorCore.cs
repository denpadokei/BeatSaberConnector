using BeatSaberConnector.SmpleJsons;
using BeatSaberConnector.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Yukarinette;

namespace BeatSaberConnector.Models
{
    public class BSConnectorCore : IYukarinetteFilterInterface, IDisposable
    {
        public BSConnectorCore()
        {
            Application.Current.Exit += this.AppExit;
            this.client.OnOpen += this.Client_OnOpen;
            this.client.OnSended += this.Client_OnSended;
            this.refreshThread = new Thread(new ThreadStart(() =>
            {
                while (this.disposedValue == false) {
                    try {
                        if ((DateTime.Now - this._lastSendDate).TotalSeconds > 4 && !string.IsNullOrEmpty(this._lastMessage)) {
                            this._lastMessage = "";
                            var json = new JSONObject();
                            json["type"] = "Hidden";
                            json["text"] = "";
                            json["sended-at"] = $"{DateTime.Now}";
                            this.client.SendTextAsync($"{json}");
                        }
                        else {
                            continue;
                        }
                    }
                    catch (Exception) {
                    }
                    finally {
                        Thread.Sleep(10);
                    }
                }
            }));
            
            this.refreshThread.Start();
        }

        private void AppExit(object sender, ExitEventArgs e)
        {
            this.Dispose();
        }

        public override string Name => Assembly.GetExecutingAssembly().GetName().Name;

        public override YukarinetteFilterPluginResult Filtering(string text, YukarinetteWordDetailData[] words)
        {

            var result = new YukarinetteFilterPluginResult();
            result.Type = YukarinetteFilterPluginResult.FilterResultType.Normal;
            result.Text = text;
            this._lastSendDate = DateTime.Now;
            this._lastMessage = text;
            NotificationStateMessage?.Invoke(text);
            var json = new JSONObject();
            json["type"] = "Show";
            json["text"] = text;
            json["sended-at"] = $"{DateTime.Now}";
            client.SendTextAsync($"{json}");
            return result;
        }

        public event Action<string> NotificationStateMessage;

        public override UserControl GetSettingUserControl()
        {
            this.connector.DataContext = new BeatSaberConnentViewModel(this);
            return this.connector;
        }

        private void Client_OnSended(object sender, SendedEvetArgs e)
        {
            this.NotificationStateMessage?.Invoke($"[{DateTime.Now} : {e.Message}]");
        }
        private void Client_OnOpen(object sender, EventArgs e)
        {
            this.NotificationStateMessage?.Invoke($"[{DateTime.Now} : BeatSaberに接続しました。]");
        }

        private BSWebSocketClient client = new BSWebSocketClient();
        private BeatSaberConnectorView connector = new BeatSaberConnectorView();
        private Thread refreshThread;
        private bool disposedValue;
        private string _lastMessage;
        private DateTime _lastSendDate;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    this.client.OnOpen -= this.Client_OnOpen;
                    this.client.OnSended -= this.Client_OnSended;
                    this.client.Dispose();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~BSConnectorCore()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
