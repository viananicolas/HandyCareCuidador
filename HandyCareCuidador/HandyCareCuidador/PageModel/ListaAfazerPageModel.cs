﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using HandyCareCuidador.Helper;
using HandyCareCuidador.Interface;
using HandyCareCuidador.Model;
using PropertyChanged;
using Xamarin.Forms;

namespace HandyCareCuidador.PageModel
{
    [ImplementPropertyChanged]
    public class ListaAfazerPageModel : FreshBasePageModel
    {
        private readonly IConclusaoAfazerRestService _conclusaoRestService;

        private bool _finalizarAfazer;
        private readonly IAfazerRestService _restService;

        private Afazer _selectedAfazer;
        private Afazer _selectedAfazerConcluido;
        public bool AfazerConcluido;
        public Task check;
        public bool deleteVisible;

        public ListaAfazerPageModel()
        {
            _restService = FreshIOC.Container.Resolve<IAfazerRestService>();
            _conclusaoRestService = FreshIOC.Container.Resolve<IConclusaoAfazerRestService>();
        }

        public HorarioViewModel oHorario { get; set; }
        public ObservableCollection<Afazer> Afazeres { get; set; }
        public ObservableCollection<Afazer> ConcluidosAfazeres { get; set; }

        public ObservableCollection<ConclusaoAfazer> AfazeresConcluidos { get; set; }
        public Afazer AfazerSelecionado { get; set; }

        public Command AddAfazer
        {
            get
            {
                return new Command(async () =>
                {
                    deleteVisible = false;
                    await CoreMethods.PushPageModel<AfazerPageModel>();
                });
            }
        }

        public Afazer SelectedAfazer
        {
            get { return _selectedAfazer; }
            set
            {
                _selectedAfazer = value;
                if (value != null)
                {
                    AfazerSelected.Execute(value);
                    SelectedAfazer = null;
                }
            }
        }

        public Command<Afazer> AfazerSelected
        {
            get
            {
                return new Command<Afazer>(async afazer =>
                {
                    deleteVisible = true;
                    var a = AfazerSelecionado;
                    RaisePropertyChanged("IsVisible");
                    await CoreMethods.PushPageModel<AfazerPageModel>(afazer);
                    afazer = null;
                });
            }
        }

        public bool FinalizarAfazer
        {
            get { return _finalizarAfazer; }
            set
            {
                _finalizarAfazer = value;
                var a = AfazerSelecionado;
                AfazerConcluido = true;
                AfazerFinalizado.Execute(value);
            }
        }

        public Command<Afazer> AfazerFinalizado
        {
            get
            {
                return new Command<Afazer>(async afazer =>
                {
                    var a = afazer;
                    await CoreMethods.PushPageModel<AfazerPageModel>(afazer);
                    afazer = null;
                });
            }
        }

        public override async void Init(object initData)
        {
            base.Init(initData);
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            AfazerSelecionado = new Afazer();
            oHorario = new HorarioViewModel();
            await GetAfazeresConcluidos();
            await GetAfazeres();
            oHorario.ActivityRunning = false;

        }
        public Command RetornarMenu
        {
            get
            {
                return new Command(async () => {
                 await CoreMethods.PushPageModel<MainMenuPageModel>();
                });
            }
        }

        public async Task GetAfazeresConcluidos()
        {
            try
            {
                await Task.Run(async () =>
                {
                    AfazeresConcluidos =
                        new ObservableCollection<ConclusaoAfazer>(await _conclusaoRestService.RefreshDataAsync());
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GetAfazeres()
        {      
            //INSERIR PACID EM MATERIAL E MEDICAMENTO
            try
            {
                await Task.Run(async () =>
                {
                    oHorario.ActivityRunning = true;
                    oHorario.FinalizarAfazer = false;
                    var selection = new ObservableCollection<Afazer>(await _restService.RefreshDataAsync());
                    if (selection.Count > 0 && AfazeresConcluidos.Count>0)
                    {
                        var result = selection.Where(e => !AfazeresConcluidos.Select(m => m.ConAfazer)
                            .Contains(e.Id)).AsEnumerable();
                        Afazeres = new ObservableCollection<Afazer>(result);
                    }
                    else
                    {
                        Afazeres = new ObservableCollection<Afazer>(selection);
                    }
                    oHorario.ActivityRunning = false;
                });
            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
        }

        public override async void ReverseInit(object returndData)
        {
            base.ReverseInit(returndData);
            var newAfazer = returndData as Afazer;
            if (!Afazeres.Contains(newAfazer))
            {
                Afazeres.Add(newAfazer);
            }
            else
            {
                await GetAfazeresConcluidos();
                //await GetAfazeres();
            }
            //if (AfazerConcluido)
            //{
            //    Task.Run(async () => await GetAfazeresConcluidos());
            //    Task.Run(async () => await GetAfazeres());
            //}
        }

        public void OnItemToggled(object sender, ToggledEventArgs e)
        {
            var toggledSwitch = (ListSwitch) sender;
        }
    }
}