import * as yup from 'yup';

export const ProfileSchema = yup.object().shape({
  firstName: yup.string().max(128).required().label('First name'),
  lastName: yup.string().max(128).required().label('Last name'),
  email: yup.string().email().max(512).required().label('Email'),
  organization: yup.string().max(512).required().label('Organization'),
  phoneNumber: yup
    .string()
    .max(12)
    .matches(/^[+]\d{11}$/, 'The phone number is incorrect')
    .required()
    .label('Phone'),
  mailingAddress: yup.string().max(512).required().label('Address'),
  city: yup.string().max(128).required().label('City'),
  state: yup.string().max(128).required().label('State'),
  zipCode: yup.string().max(64).required().label('Zip'),
});

export const SiteSchema = yup.object().shape({
  name: yup.string().max(512).required().label('Name'),
  ownership: yup.string().max(512).required().label('ownership'),
  naics: yup
    .string()
    .matches(/^\d{6}$/, 'NAICS codes must be a 6 digit code')
    .required()
    .label('NAICS code'),
  naicsTitle: yup.string().max(512).required().label('NAICS title'),
  activity: yup.string().min(10).max(512).required().label('business activity'),
});

export const ContactSchema = yup.object().shape({
  type: yup.string(),
  fullName: yup.string(),
  email: ProfileSchema.email,
  organization: ProfileSchema.organization,
  phoneNumber: ProfileSchema.phoneNumber,
  mailingAddress: ProfileSchema.mailingAddress,
  city: ProfileSchema.city,
  state: ProfileSchema.state,
  zipCode: ProfileSchema.zipCode,
});
