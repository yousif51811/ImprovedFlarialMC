using Flarial.Controls;
using System.Net.Http;

namespace Flarial
{
    internal class UIContainer
    {
        public static ModalContainer Modal = new ModalContainer();
    }
    internal class  Container
    {
        public static readonly HttpClient Client = new HttpClient();
    }
}
