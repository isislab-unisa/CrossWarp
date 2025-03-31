<a id="readme-top"></a>

<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![project_license][license-shield]][license-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
<h1 align="center">CrossWarp</h1>

  <p align="center">
    A cross-reality framework for seamless collaboration between different AR/VR users.
    <br />
    <br />
    <a href="https://github.com/isislab-unisa/CrossWarp">View Demo</a>
    &middot;
    <a href="https://github.com/isislab-unisa/CrossWarp/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/isislab-unisa/CrossWarp/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#-about-the-project">About The Project</a>
      <ul>
        <li><a href="#-built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#-getting-started">Getting Started</a>
      <ul>
        <li><a href="#-prerequisites">Prerequisites</a></li>
        <li><a href="#-installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#-usage">Usage</a></li>
    <li><a href="#-roadmap">Roadmap</a></li>
    <li><a href="#-contributing">Contributing</a></li>
    <li><a href="#-license">License</a></li>
    <li><a href="#-contact">Contact</a></li>
    <li><a href="#-acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## 📖 About The Project

![CrossWarp][product-screenshot]



CrossWarp is a framework that unifies mobile AR devices (Android and iOS) and desktops into a single collaborative ecosystem. It not only ensures seamless transitions between the physical and virtual worlds but also provides a device-independent architecture designed to maximize interoperability. The system enables co-present users, such as two people using AR-enabled smartphones, to manipulate shared objects in real time while preserving their state and context, regardless of the platform used.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



### 🛠 Built With

* [![Unity][Unity]][Unity-url]
* [![ARFoundation][ARFoundation]][ARFoundation-url]
* [![XRIT][XRIT]][XRIT-url]
* [![Photon][Photon]][Photon-url]
* ![Csharp][Csharp]

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## 🚀 Getting Started

To get a local copy up and running follow these simple example steps.

### 📌 Prerequisites

Unity 2022.3 or later must be installed, to install it you can use Unity Hub.

### 🔧 Installation

1. Install Photon Fusion following the instructions on the Photon website (https://doc.photonengine.com/fusion/current/getting-started/sdk-download).
    - Add your AppID in the Photon Fusion configuration
2. Install CrossWarp via the Unity Package Manager by following these steps:
    - Open the Package Manager.
    - Click on "Add package from git URL".
    - Enter the following URL: https://github.com/isislab-unisa/CrossWarp.git?path=/Assets/CrossWarp

### ⚙️ Configuration
After importing, some parameters need to be configured in Project Settings:
  - In XR Plugin Management, enable ARCore or ARKit, depending on the target platform.
  - In XR Plugin Management, run Project Validation to detect and fix any configuration issues.
  - In Player → Other Settings, disable the Vulkan Graphics API.
  - In Player → Other Settings, change the Scripting Backend to IL2CPP and enable ARM64 support.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## 🎮 Usage

Once the installation is complete, you can use the components provided by CrossWarp to develop Cross-Reality experiences. The package includes example scenes demonstrating the core components, including the integration of interactive objects with physics mechanics. These examples can be imported and used as a foundation for building new collaborative and immersive applications.

By examining the HouseScene included in the package, you can see how to configure the environment for Cross-Reality experiences. The key components are:
- AR Scene Objects: XR Interaction Manager, AR Session, XR Origin
- Photon Configuration Objects: Prototype Network Start, Prototype Runner

![Product Name Screen Shot][housescene]

The House object contains all the static elements of the house structure that do not interact with CrossWarp. In contrast, objects like SofaDouble, SofaArmChair, TableRectangleShort, and LampTall are designed to be transported between different realities.

### How To Configure Objects
To enable an object to be transported between different realities, you need to configure specific components within it:
- AR Anchor
- Outline
- Collider (customized based on the object)
- Network Object
  - with AllowStateAuthority set to true
- Movable Object
- Transition Manager

With these components configured, any type of object can seamlessly transition between the two worlds.


<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## 🛤 Roadmap

- [ ] Support for Head-Mounted Displays VR/AR
- [ ] ContainerObjects
- [ ] Physical objects prefabs

See the [open issues](https://github.com/isislab-unisa/CrossWarp/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## 🤝 Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://github.com/isislab-unisa/CrossWarp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=isislab-unisa/CrossWarp" alt="contrib.rocks image" />
</a>



<!-- LICENSE -->
## 📜 License

Distributed under the Apache License 2.0. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## 📧 Contact

Vincenzo Offertucci - [Linkedin][linkedin-url]

Project Link: [https://github.com/isislab-unisa/CrossWarp](https://github.com/isislab-unisa/CrossWarp)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## 🎖️ Acknowledgments

* QuickOutline package from Chris Nolet (https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488)
* In-game Debug Console package from yasirkula (https://assetstore.unity.com/packages/tools/gui/in-game-debug-console-68068) useful if you want an in-game debugger on mobile 
* Interior House Assets package (https://assetstore.unity.com/packages/3d/environments/interior-house-assets-urp-257122)
* Digital artworks made by Federica Corso

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/isislab-unisa/CrossWarp.svg?style=for-the-badge
[contributors-url]: https://github.com/isislab-unisa/CrossWarp/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/isislab-unisa/CrossWarp.svg?style=for-the-badge
[forks-url]: https://github.com/isislab-unisa/CrossWarp/network/members
[stars-shield]: https://img.shields.io/github/stars/isislab-unisa/CrossWarp.svg?style=for-the-badge
[stars-url]: https://github.com/isislab-unisa/CrossWarp/stargazers
[issues-shield]: https://img.shields.io/github/issues/isislab-unisa/CrossWarp.svg?style=for-the-badge
[issues-url]: https://github.com/isislab-unisa/CrossWarp/issues
[license-shield]: https://img.shields.io/github/license/isislab-unisa/CrossWarp.svg?style=for-the-badge
[license-url]: https://github.com/isislab-unisa/CrossWarp/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/vincenzo-offertucci
[product-screenshot]: ReadmeFigures/CrossWarp.png
[product-video]: ReadmeFigures/transitionsdemo.mp4
[housescene]: ReadmeFigures/HouseScene.png



[Unity]: https://img.shields.io/badge/unity-000000?style=for-the-badge&logo=unity&logoColor=white
[Unity-url]: https://unity.com/

[ARFoundation]: https://img.shields.io/badge/ARFoundation-282828?style=for-the-badge
[ARFoundation-url]: https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html

[XRIT]: https://img.shields.io/badge/XR%20Interaction%20Toolkit-333333?style=for-the-badge
[XRIT-url]: https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.1/manual/index.html

[Photon]: https://img.shields.io/badge/Photon%20Fusion-004480?style=for-the-badge&logo=photon&logoColor=white
[Photon-url]: https://www.photonengine.com/Fusion

[Csharp]: https://img.shields.io/badge/C%23-00C244?style=for-the-badge
