﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using HandyCareCuidador.Data;
using HandyCareCuidador.Helper;
using HandyCareCuidador.Model;
using Plugin.Media;
using Plugin.Media.Abstractions;
using PropertyChanged;
using Xamarin.Forms;

namespace HandyCareCuidador.PageModel
{
    [ImplementPropertyChanged]
    public class CuidadorPageModel : FreshBasePageModel
    {
        private TipoCuidador _tipoCuidador;
        private App app;
        public Cuidador Cuidador { get; set; }
        public ObservableCollection<TipoCuidador> TiposCuidadores { get; set; }
        public ValidacaoCuidador ValidacaoCuidador { get; set; }
        public PageModelHelper oHorario { get; set; }
        public ImageSource CuidadorFoto { get; set; }
        public ImageSource Documento { get; set; }

        public TipoCuidador SelectedTipoCuidador
        {
            get { return _tipoCuidador; }
            set
            {
                _tipoCuidador = value;
                if (value != null)
                {
                    //ShowMedicamentos.Execute(value);
                    //SelectedPaciente = null;
                }
            }
        }

        public Command FotoDoc
        {
            get
            {
                return new Command(async () =>
                {
                    var result =
                        await CoreMethods.DisplayActionSheet("Forma de fotografia", "Cancelar", null, "Galeria",
                            "Tirar foto");

                    switch (result)
                    {
                        case "Tirar foto":
                        {
                            var image = new Image();
                            await CrossMedia.Current.Initialize();

                            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                            {
                                await CoreMethods.DisplayAlert("No Camera", ":( No camera available.", "OK");
                                return;
                            }

                            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                            {
                                Directory = "Handy Care Fotos",
                                Name = DateTime.Now + "HandyCareFoto.jpg",
                                CompressionQuality = 10,
                                PhotoSize = PhotoSize.Medium,
                                SaveToAlbum = true
                            });
                            if (file == null)
                                return;
                            await CoreMethods.DisplayAlert("File Location", file.Path, "OK");
                            ValidacaoCuidador.ValDocumento = HelperClass.ReadFully(file.GetStream());
                            Documento = ImageSource.FromStream(() =>
                            {
                                var stream = file.GetStream();
                                file.Dispose();
                                return stream;
                            });
                        }
                            break;
                        case "Galeria":
                        {
                            var image = new Image();

                            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                            {
                                CompressionQuality = 10,
                                PhotoSize = PhotoSize.Medium
                            });
                            if (file == null)
                                return;
                            await CoreMethods.DisplayAlert("File Location", file.Path, "OK");
                            ValidacaoCuidador.ValDocumento = HelperClass.ReadFully(file.GetStream());
                            image.Source = ImageSource.FromStream(() =>
                            {
                                var stream = file.GetStream();
                                file.Dispose();
                                return stream;
                            });
                        }
                            break;
                    }
                });
            }
        }

        public Command FotoCui
        {
            get
            {
                return new Command(async () =>
                {
                    var result =
                        await CoreMethods.DisplayActionSheet("Forma de fotografia", "Cancelar", null, "Galeria",
                            "Tirar foto");

                    switch (result)
                    {
                        case "Tirar foto":
                        {
                            var image = new Image();
                            await CrossMedia.Current.Initialize();

                            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                            {
                                await CoreMethods.DisplayAlert("No Camera", ":( No camera available.", "OK");
                                return;
                            }

                            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                            {
                                Directory = "Handy Care Fotos",
                                Name = DateTime.Now + "HandyCareFoto.jpg",
                                CompressionQuality = 10,
                                PhotoSize = PhotoSize.Medium,
                                SaveToAlbum = true
                            });
                            if (file == null)
                                return;
                            await CoreMethods.DisplayAlert("File Location", file.Path, "OK");
                            Cuidador.CuiFoto = HelperClass.ReadFully(file.GetStream());
                            CuidadorFoto = ImageSource.FromStream(() =>
                            {
                                var stream = file.GetStream();
                                file.Dispose();
                                return stream;
                            });
                        }
                            break;
                        case "Galeria":
                        {
                            var image = new Image();

                            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                            {
                                CompressionQuality = 10,
                                PhotoSize = PhotoSize.Medium
                            });
                            if (file == null)
                                return;
                            await CoreMethods.DisplayAlert("File Location", file.Path, "OK");
                            Cuidador.CuiFoto = HelperClass.ReadFully(file.GetStream());
                            CuidadorFoto = ImageSource.FromStream(() =>
                            {
                                var stream = file.GetStream();
                                file.Dispose();
                                return stream;
                            });
                        }
                            break;
                    }
                });
            }
        }

        public Command SaveCommand
        {
            get
            {
                return new Command(async () =>
                {
                    oHorario.Visualizar = false;
                    oHorario.ActivityRunning = true;
                    ValidacaoCuidador.Id = Guid.NewGuid().ToString();
                    ValidacaoCuidador.ValValidado = true;
                    Cuidador.CuiValidacaoCuidador = ValidacaoCuidador.Id;
                    Cuidador.CuiTipoCuidador = SelectedTipoCuidador.Id;
                    await
                        CuidadorRestService.DefaultManager.SaveValidacaoCuidadorAsync(ValidacaoCuidador,
                            oHorario.NovoCuidador);
                    await CuidadorRestService.DefaultManager.SaveCuidadorAsync(Cuidador, oHorario.NovoCuidador);
                    if (oHorario.NovoCuidador)
                        app.AbrirMainMenu(Cuidador);
                    else
                        await CoreMethods.PopPageModel(Cuidador);

                    //                        await CoreMethods.PushPageModelWithNewNavigation<MainMenuPageModel>(Cuidador);
                });
            }
        }

        public override async void Init(object initData)
        {
            base.Init(initData);
            Cuidador = new Cuidador();
            SelectedTipoCuidador = new TipoCuidador();
            ValidacaoCuidador = new ValidacaoCuidador();
            oHorario = new PageModelHelper
            {
                ActivityRunning = true,
                Visualizar = false,
                VisualizarTermino = false,
                NovoCuidador = false,
                NovoCadastro = false,
                CuidadorExibicao = true
            };
            var x = initData as Tuple<Cuidador, App>;
            if (x != null)
            {
                Cuidador = x.Item1;
                if (x.Item2 != null)
                    app = x.Item2;
            }
            //Cuidador = initData as Cuidador;
            oHorario.NovoCuidador = Cuidador?.Id == null;
            oHorario.NovoCadastro = Cuidador?.Id == null;
            oHorario.CuidadorExibicao = Cuidador?.Id != null;
            await GetData();
            oHorario.ActivityRunning = false;
            oHorario.Visualizar = true;
        }


        private async Task GetData()
        {
            try
            {
                await Task.Run(async () =>
                {
                    TiposCuidadores =
                        new ObservableCollection<TipoCuidador>(
                            await CuidadorRestService.DefaultManager.RefreshTipoCuidadorAsync());
                    var x = TiposCuidadores.Count;
                    //var x =
                    //    new ObservableCollection<TipoCuidador>(
                    //        await CuidadorRestService.DefaultManager.RefreshTipoCuidadorAsync());
                    if (!oHorario.NovoCuidador)
                    {
                        SelectedTipoCuidador = TiposCuidadores.FirstOrDefault(e => e.Id == Cuidador.CuiTipoCuidador);
                        ValidacaoCuidador = new ObservableCollection<ValidacaoCuidador>(
                                await CuidadorRestService.DefaultManager.RefreshValidacaoCuidadorAsync())
                            .FirstOrDefault(e => e.Id == Cuidador.CuiValidacaoCuidador);
                    }
                });
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}