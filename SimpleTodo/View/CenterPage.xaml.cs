﻿using System;
using System.Collections.Generic;

using RxRouting;
using Xamarin.Forms;

namespace SimpleTodo
{
    public partial class CenterPage : NavigationPage
    {
        private CenterPageModel model = new CenterPageModel(Application.Current.RealmAccess());

        private TabListTransitObserver tabListTransitTarget;
        private TabJumpingObserver tabJumpingTarget;
        private CentralViewChangeObserver centralViewChangeTarget;

        public CenterPage(Page childPage) : base(childPage)
        {
            InitializeComponent();

            BindingContext = model;

            tabListTransitTarget = new TabListTransitObserver(async _ => await PushAsync(new TabMaintenancePage()));
            tabJumpingTarget = new TabJumpingObserver(async _ => await PopAsync());
            centralViewChangeTarget = new CentralViewChangeObserver(c => model.OnCentralViewChanged(c));

            var router = Application.Current.ReactionRouter();
            router.AddReactiveTarget((int)RxSourceEnum.TabListTransit, tabListTransitTarget);
            router.AddReactiveTarget((int)RxSourceEnum.TabJumping, tabJumpingTarget);
            router.AddReactiveTarget((int)RxSourceEnum.CentralViewChange, centralViewChangeTarget);
        }
    }
}
