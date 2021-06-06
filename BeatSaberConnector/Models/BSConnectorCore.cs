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
            Application.Current.Exit += this.Current_Exit;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            this.Dispose();
        }

        public override string Name => Assembly.GetExecutingAssembly().GetName().Name;

        public override YukarinetteFilterPluginResult Filtering(string text, YukarinetteWordDetailData[] words)
        {

            var result = new YukarinetteFilterPluginResult();
            result.Type = YukarinetteFilterPluginResult.FilterResultType.Normal;
            result.Text = text;
            client.SendText(text);
            return result;
        }

        public event Action<string> NotificationStateMessage;

        public override UserControl GetSettingUserControl()
        {
            this.connector.DataContext = new BeatSaberConnentViewModel(this);
            return this.connector;
        }

        public override void Loaded()
        {
            this.client.OnOpen += this.Client_OnOpen;
            this.client.OnSended += this.Client_OnSended;

            this.connentThread = new Thread(new ThreadStart(() =>
            {
                while (this.disposedValue == false) {
                    try {
                        if (this.client.ClientSocket.State != WebSocket4Net.WebSocketState.Open) {
                            continue;
                        }
                        else {
                            this.NotificationStateMessage?.Invoke("BeatSaberに接続します。");
                            _ = Task.Run(this.client.ClientSocket.Open);
                        }
                    }
                    catch (Exception e) {
                        this.NotificationStateMessage?.Invoke($"{e}");
                    }
                    finally {
                        Thread.Sleep(3000);
                    }
                }
            }));
            this.connentThread.Start();
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
        private Thread connentThread;
        private bool disposedValue;

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
