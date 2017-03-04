using EContract.Dssp.Client.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EContract.Dssp.Client
{
    /// <summary>
    /// Additional properties that will be added to the signature
    /// </summary>
    public class SignatureRequestProperties : SignatureProperties
    {
        /// <summary>
        /// Set to visualize the signature
        /// </summary>
        public VisibleSignatureProperties VisibleSignature { get; set; }


        internal VisibleSignatureConfigurationType Configuration
        {
            get
            {
                var items = new List<VisibleSignatureItemType>();
                PixelVisibleSignaturePositionType pixelVisibleSignaturePosition = null;

                if (!string.IsNullOrEmpty(SignerRole))
                {
                    items.Add(new VisibleSignatureItemType()
                    {
                        ItemName = ItemNameEnum.SignatureReason,
                        ItemValue = new ItemValueStringType()
                        {
                            ItemValue = SignerRole
                        }
                    });
                }
                if (!string.IsNullOrEmpty(SignatureProductionPlace))
                {
                    items.Add(new VisibleSignatureItemType()
                    {
                        ItemName = ItemNameEnum.SignatureProductionPlace,
                        ItemValue = new ItemValueStringType()
                        {
                            ItemValue = SignatureProductionPlace
                        }
                    });
                }

                if (VisibleSignature != null)
                {
                    if (VisibleSignature is ImageVisibleSignature photoProp)
                    {
                        items.Add(new VisibleSignatureItemType()
                        {
                            ItemName = ItemNameEnum.SignerImage,
                            ItemValue = new ItemValueURIType()
                            {
                                ItemValue = photoProp.ValueUri
                            }
                        });

                        if (!string.IsNullOrEmpty(photoProp.CustomText))
                        {
                            items.Add(new VisibleSignatureItemType()
                            {
                                ItemName = ItemNameEnum.CustomText,
                                ItemValue = new ItemValueStringType()
                                {
                                    ItemValue = photoProp.CustomText
                                }
                            });
                        }
                    }
                    else
                    {
                        throw new ArgumentException("The type of VisibleSignatureProperties (field of SignatureRequestProperties) is unsupported", "properties");
                    }

                    pixelVisibleSignaturePosition = new PixelVisibleSignaturePositionType()
                    {
                        PageNumber = VisibleSignature.Page,
                        x = VisibleSignature.X,
                        y = VisibleSignature.Y
                    };
                }

                if (items.Count == 0) return null;
                return new VisibleSignatureConfigurationType()
                {
                    VisibleSignaturePolicy = VisibleSignaturePolicyType.DocumentSubmissionPolicy,
                    VisibleSignatureItemsConfiguration = new VisibleSignatureItemsConfigurationType()
                    {
                        VisibleSignatureItem = items.ToArray<VisibleSignatureItemType>()
                    },
                    VisibleSignaturePosition = pixelVisibleSignaturePosition
                };
            }
        }
    }
}
