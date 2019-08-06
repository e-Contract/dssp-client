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
                            ItemValueType itemValue;
                            if (string.IsNullOrEmpty(photoProp.CustomText2) &&
                                string.IsNullOrEmpty(photoProp.CustomText3) &&
                                string.IsNullOrEmpty(photoProp.CustomText4) &&
                                string.IsNullOrEmpty(photoProp.CustomText5))
                            {
                                itemValue = new ItemValueStringType()
                                {
                                    ItemValue = photoProp.CustomText
                                };
                            }
                            else
                            {
                                itemValue = new ItemValueStringsType()
                                {
                                    ItemValue1 = photoProp.CustomText,
                                    ItemValue2 = photoProp.CustomText2,
                                    ItemValue3 = photoProp.CustomText3,
                                    ItemValue4 = photoProp.CustomText4,
                                    ItemValue5 = photoProp.CustomText5
                                };
                            }

                            items.Add(new VisibleSignatureItemType()
                            {
                                ItemName = ItemNameEnum.CustomText,
                                ItemValue = itemValue
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
