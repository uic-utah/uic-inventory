import { LimitedTextarea, LimitedTextareaFileInput } from './LimitedTextarea';

export default {
  title: 'FormElements/Limited Textarea',
  component: LimitedTextarea,
};

export const Default = () => <LimitedTextarea limit={500} rows="5" register={() => {}} errors={{ errors: {} }} />;
export const FileInputDefault = () => <LimitedTextareaFileInput />;
