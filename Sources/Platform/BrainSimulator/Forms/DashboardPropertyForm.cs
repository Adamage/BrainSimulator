﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GoodAI.BrainSimulator.DashboardUtils;
using GoodAI.Core.Dashboard;
using GoodAI.Core.Execution;
using GoodAI.Core.Nodes;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.BrainSimulator.Forms
{
    public partial class DashboardPropertyForm : DockContent
    {
        private readonly MainForm m_mainForm;

        public event PropertyValueChangedEventHandler PropertyValueChanged
        {
            add
            {
                propertyGrid.PropertyValueChanged += value;
                propertyGridGrouped.PropertyValueChanged += value;
            }
            remove
            {
                propertyGrid.PropertyValueChanged -= value;
                propertyGridGrouped.PropertyValueChanged -= value;
            }
        }

        public DashboardPropertyForm(MainForm mainForm)
        {
            m_mainForm = mainForm;
            InitializeComponent();
            DisableGroupButtons();

            propertyGrid.PropertyValueChanged += OnPropertyValueChanged;
            propertyGridGrouped.PropertyValueChanged += OnGroupPropertyValueChanged;
        }

        private void OnGroupPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ProxyPropertyGroupDescriptor descriptor = GetCurrentGroupDescriptor();
            foreach (MyNode node in descriptor.Proxy.SourceProperty.GroupedProperties.Select(member => member.Node))
                RefreshNode(node);
        }

        private void OnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            ProxyPropertyDescriptor descriptor = GetCurrentPropertyDescriptor();
            MyNode node = descriptor.Proxy.SourceProperty.Node;
            RefreshNode(node);
        }

        private void RefreshNode(MyNode node)
        {
            node.Updated();

            propertyGrid.Refresh();
            m_mainForm.InvalidateGraphLayouts();
        }

        private DashboardViewModel DashboardViewModel
        {
            get { return propertyGrid.SelectedObject as DashboardViewModel; }
            set
            {
                if (DashboardViewModel != null)
                    DashboardViewModel.PropertyChanged -= OnDashboardPropertiesChanged;

                propertyGrid.SelectedObject = value;
                value.PropertyChanged += OnDashboardPropertiesChanged;
            }
        }

        private GroupedDashboardViewModel GroupedDashboardViewModel
        {
            get { return propertyGridGrouped.SelectedObject as GroupedDashboardViewModel; }
            set
            {
                if (GroupedDashboardViewModel != null)
                    GroupedDashboardViewModel.PropertyChanged -= OnGroupedDashboardPropertiesChanged;

                propertyGridGrouped.SelectedObject = value;
                value.PropertyChanged += OnGroupedDashboardPropertiesChanged;
            }
        }

        public void SetDashboards(Dashboard dashboard, GroupDashboard groupedDashboard)
        {
            DashboardViewModel = new DashboardViewModel(dashboard);
            GroupedDashboardViewModel = new GroupedDashboardViewModel(groupedDashboard);
        }

        private bool CanEditNodeProperties
        {
            set
            {
                foreach (SingleProxyProperty propertyProxy in DashboardViewModel.GetProperties(new Attribute[0])
                    .Cast<ProxyPropertyDescriptor>()
                    .Select(descriptor => descriptor.Proxy)
                    .Where(proxy => proxy.Target is MyNode))
                {
                    propertyProxy.ReadOnly = !value;
                }

                foreach (ProxyPropertyGroup groupProxy in GroupedDashboardViewModel.GetProperties(new Attribute[0])
                    .Cast<ProxyPropertyGroupDescriptor>()
                    .Select(descriptor => descriptor.Proxy)
                    .Where(proxy => proxy.SourceProperty.GroupedProperties.Any(property => property.Target is MyNode)))
                {
                    groupProxy.ReadOnly = !value;
                }

                propertyGrid.Refresh();
                propertyGridGrouped.Refresh();
            }
        }

        private void OnDashboardPropertiesChanged(object sender, EventArgs args)
        {
            SetPropertyGridButtonsEnabled(false);
            propertyGrid.Refresh();
        }

        private void SetPropertyGridButtonsEnabled(bool enabled)
        {
            removeButton.Enabled = enabled;
            goToNodeButton.Enabled = enabled;
        }

        private void OnGroupedDashboardPropertiesChanged(object sender, EventArgs args)
        {
            DisableGroupButtons();
            propertyGridGrouped.Refresh();
        }

        private void DisableGroupButtons()
        {
            SetPropertyGridGroupedButtonsEnabled(false);
        }

        private void SetPropertyGridGroupedButtonsEnabled(bool enabled)
        {
            removeGroupButton.Enabled = enabled;
            editGroupButton.Enabled = enabled;
            addToGroupButton.Enabled = enabled;
            removeFromGroupButton.Enabled = enabled;
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            ProxyPropertyDescriptor descriptor = GetCurrentPropertyDescriptor();

            DashboardViewModel.RemoveProperty(descriptor.Proxy);
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            ClearError();

            if (GroupedDashboardViewModel == null)
                return;

            if (e.NewSelection != null)
                SetPropertyGridButtonsEnabled(true);
        }

        private void ClearError()
        {
            errorText.Text = "";
        }

        private void addGroupButton_Click(object sender, EventArgs e)
        {
            GroupedDashboardViewModel.AddGroupedProperty();
        }

        private void removeGroupButton_Click(object sender, EventArgs e)
        {
            ProxyPropertyGroupDescriptor descriptor = GetCurrentGroupDescriptor();

            GroupedDashboardViewModel.RemoveProperty(descriptor.Proxy);
            propertyGrid.Refresh();
        }

        private ProxyPropertyDescriptor GetCurrentPropertyDescriptor()
        {
            var descriptor = propertyGrid.SelectedGridItem.PropertyDescriptor as ProxyPropertyDescriptor;
            if (descriptor == null)
                throw new InvalidOperationException("Invalid property descriptor used in the dashboard.");
            return descriptor;
        }

        private ProxyPropertyGroupDescriptor GetCurrentGroupDescriptor()
        {
            var descriptor =
                propertyGridGrouped.SelectedGridItem.PropertyDescriptor as ProxyPropertyGroupDescriptor;
            if (descriptor == null)
                throw new InvalidOperationException("The group property grid contained an invalid descriptor.");

            return descriptor;
        }

        private void editGroupButton_Click(object sender, EventArgs e)
        {
            ProxyPropertyGroupDescriptor descriptor = GetCurrentGroupDescriptor();

            var dialog = new DashboardGroupNameDialog(propertyGridGrouped, descriptor.Proxy.SourceProperty);
            dialog.ShowDialog();
        }

        private void addToGroupButton_Click(object sender, EventArgs e)
        {
            // This can be null if the category was selected.
            var selectedPropertyDescriptor = propertyGrid.SelectedGridItem.PropertyDescriptor as ProxyPropertyDescriptor;
            if (selectedPropertyDescriptor == null)
                return;

            DashboardNodeProperty property = selectedPropertyDescriptor.Proxy.SourceProperty;

            ProxyPropertyGroupDescriptor selectedGroupDescriptor = GetCurrentGroupDescriptor();
            DashboardPropertyGroup groupProperty = selectedGroupDescriptor.Proxy.SourceProperty;

            try
            {
                groupProperty.Add(property);

                propertyGrid.Refresh();
                SetPropertyGridButtonsEnabled(false);
                propertyGridGrouped.Refresh();
                memberListBox.Refresh();
            }
            catch (InvalidOperationException)
            {
                errorText.Text = string.Format("Cannot add a {0} property to a {1} group",
                    selectedPropertyDescriptor.PropertyType.Name,
                    selectedGroupDescriptor.PropertyType.Name);
            }
        }

        private void propertyGridGrouped_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            ClearError();

            memberListBox.Items.Clear();
            if (e.NewSelection != null)
            {
                SetPropertyGridGroupedButtonsEnabled(true);

                LoadGroupedProperties(e.NewSelection.PropertyDescriptor as ProxyPropertyGroupDescriptor);
            }
            propertyGrid.Refresh();
        }

        private void LoadGroupedProperties(ProxyPropertyGroupDescriptor groupDescriptor)
        {
            foreach (var proxy in groupDescriptor.Proxy.GetGroupMembers())
                memberListBox.Items.Add(proxy);
        }

        private void removeFromGroupButton_Click(object sender, EventArgs e)
        {
            foreach (var proxy in memberListBox.SelectedItems.Cast<SingleProxyProperty>())
                proxy.SourceProperty.Group.Remove(proxy.SourceProperty);

            memberListBox.Refresh();
            propertyGrid.Refresh();
            propertyGridGrouped.Refresh();
        }

        public void OnSimulationStateChanged(object sender, MySimulationHandler.StateEventArgs e)
        {
            CanEditNodeProperties = e.NewState == MySimulationHandler.SimulationState.STOPPED;
        }

        private void goToNodeButton_Click(object sender, EventArgs e)
        {
            ProxyPropertyDescriptor descriptor = GetCurrentPropertyDescriptor();
            MyNode targetNode = descriptor.Proxy.SourceProperty.Node;

            if (targetNode is MyWorld)
            {
                GraphLayoutForm graphForm = m_mainForm.OpenGraphLayout(targetNode.Owner.Network);
                graphForm.worldButton_Click(sender, EventArgs.Empty);
            }
            else
            {
                GraphLayoutForm graphForm = m_mainForm.OpenGraphLayout(targetNode.Parent);
                graphForm.SelectNodeView(targetNode);
            }
        }
    }
}
