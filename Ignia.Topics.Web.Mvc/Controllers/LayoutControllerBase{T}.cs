﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;
using Ignia.Topics.Web.Mvc.Models;

namespace Ignia.Topics.Web.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: LAYOUT CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to views for populating specific layout dependencies, such as the <see cref="Menu"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     As a best practice, global data required by the layout view are requested independently of the current page. This
  ///     allows each layout element to be provided with its own layout data, in the form of <see
  ///     cref="NavigationViewModel{T}"/>s, instead of needing to add this data to every view model returned by <see
  ///     cref="TopicController"/>. The <see cref="LayoutController{T}"/> facilitates this by not only providing a default
  ///     implementation for <see cref="Menu"/>, but additionally providing protected helper methods that aid in locating and
  ///     assembling <see cref="Topic"/> and <see cref="INavigationTopicViewModel{T}"/> references that are relevant to
  ///     specific layout elements.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="LayoutController{T}"/> does not assume that a particular view
  ///     model will be used, and instead accepts a generic argument for any view model that implements the interface <see
  ///     cref="INavigationTopicViewModel{T}"/>. Since generic controllers cannot be effectively routed to, however, that means
  ///     implementors must, at minimum, provide a local instance of <see cref="LayoutController{T}"/> which sets the generic
  ///     value to the desired view model. To help enforce this, while avoiding ambiguity, this class is marked as
  ///     <c>abstract</c> and suffixed with <c>Base</c>.
  ///   </para>
  /// </remarks>
  public abstract class LayoutControllerBase<T> : AsyncController where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRoutingService            _topicRoutingService            = null;
    private                     Topic                           _currentTopic                   = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    protected LayoutControllerBase(
      ITopicRoutingService topicRoutingService,
      IHierarchicalTopicMappingService<T> hierarchicalTopicMappingService
    ) : base() {
      _topicRoutingService = topicRoutingService;
      HierarchicalTopicMappingService = hierarchicalTopicMappingService;
    }

    /*==========================================================================================================================
    | CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current topic associated with the request.
    /// </summary>
    /// <returns>The Topic associated with the current request.</returns>
    protected Topic CurrentTopic {
      get {
        if (_currentTopic == null) {
          _currentTopic = _topicRoutingService.GetCurrentTopic();
        }
        return _currentTopic;
      }
    }

    /*==========================================================================================================================
    | HIERARCHICAL TOPIC MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="IHierarchicalTopicMappingService{T}"/> associated with the request.
    /// </summary>
    /// <returns>The <see cref="IHierarchicalTopicMappingService{T}"/> associated with the current request.</returns>
    protected IHierarchicalTopicMappingService<T> HierarchicalTopicMappingService { get; }

    /*==========================================================================================================================
    | MENU
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the global menu for the site layout, which exposes the top two tiers of navigation.
    /// </summary>
    public async virtual Task<PartialViewResult> Menu() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentTopic          = CurrentTopic;
      var navigationRootTopic   = (Topic)null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify navigation root
      >-------------------------------------------------------------------------------------------------------------------------
      | The navigation root in the case of the main menu is the namespace; i.e., the first topic underneath the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      navigationRootTopic = HierarchicalTopicMappingService.GetHierarchicalRoot(currentTopic, 2, "Web");
      var navigationRoot = await HierarchicalTopicMappingService.GetRootViewModelAsync(
        navigationRootTopic,
        3,
        t => t.ContentType != "PageGroup"
      ).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationViewModel   = new NavigationViewModel<T>() {
        NavigationRoot          = navigationRoot,
        CurrentKey              = CurrentTopic?.GetUniqueKey()
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the corresponding view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return PartialView(navigationViewModel);

    }

  } // Class

} // Namespace